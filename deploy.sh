#!/usr/bin/env bash
set -euo pipefail

############################################
# Deploy - SITE (Blazor Server)
############################################
REGION="${REGION:-us-east-1}"

# ARNs do cluster/service do SITE
CLUSTER="${CLUSTER:-arn:aws:ecs:us-east-1:763548578114:cluster/fitnutri-cluster}"
SERVICE="${SERVICE:-arn:aws:ecs:us-east-1:763548578114:service/fitnutri-cluster/fitnutri-site-task-service-8sgriule}"

# ECR
ECR_REPO="${ECR_REPO:-fitnutri-site}"

# Tags
IMAGE_TAG="${1:-$(date +%Y%m%d-%H%M)-$(git rev-parse --short HEAD 2>/dev/null || echo local)}"
ALIAS_TAG="${ALIAS_TAG:-prod}"

# Build
PLATFORM="${PLATFORM:-linux/amd64}"
DOCKERFILE="${DOCKERFILE:-Dockerfile}"
BUILD_CONTEXT="${BUILD_CONTEXT:-.}"   # RAIZ

# Health check TG (opcional)
WAIT_TG_HEALTH="${WAIT_TG_HEALTH:-true}"
TIMEOUT="${TIMEOUT:-300}"
SLEEP_SEC="${SLEEP_SEC:-10}"

echo "==> Deploy SITE"
echo "Região...............: $REGION"
echo "Cluster..............: $CLUSTER"
echo "Service..............: $SERVICE"
echo "ECR Repo.............: $ECR_REPO"
echo "Tag imutável.........: $IMAGE_TAG"
echo "Alias (Task aponta)..: $ALIAS_TAG"
echo "Plataforma...........: $PLATFORM"

ACCOUNT_ID="$(aws sts get-caller-identity --query Account --output text)"
ECR_URI="${ACCOUNT_ID}.dkr.ecr.${REGION}.amazonaws.com/${ECR_REPO}"

for cmd in aws docker; do
  command -v "$cmd" >/dev/null 2>&1 || { echo "❌ '$cmd' não encontrado."; exit 1; }
done

# Valida cluster/service
STATUS_CLUSTER="$(aws ecs describe-clusters --clusters "$CLUSTER" --region "$REGION" --query 'clusters[0].status' --output text || true)"
[[ "$STATUS_CLUSTER" == "ACTIVE" ]] || { echo "❌ Cluster não ativo: $CLUSTER"; exit 1; }

FAILURES="$(aws ecs describe-services --cluster "$CLUSTER" --services "$SERVICE" --region "$REGION" --query 'failures' --output json)"
if echo "$FAILURES" | grep -q '"reason"'; then
  echo "❌ Service não encontrado no cluster."; echo "$FAILURES"; exit 1
fi

echo "==> Login no ECR"
aws ecr get-login-password --region "$REGION" \
  | docker login --username AWS --password-stdin "${ACCOUNT_ID}.dkr.ecr.${REGION}.amazonaws.com"

echo "==> Build da imagem ($PLATFORM)"
echo ">> CMD: docker buildx build --platform $PLATFORM -f $DOCKERFILE -t ${ECR_REPO}:${IMAGE_TAG} $BUILD_CONTEXT --load"
docker buildx build \
  --platform "$PLATFORM" \
  -f "$DOCKERFILE" \
  -t "${ECR_REPO}:${IMAGE_TAG}" \
  "$BUILD_CONTEXT" \
  --load

echo "==> Tagging"
docker tag "${ECR_REPO}:${IMAGE_TAG}" "${ECR_URI}:${IMAGE_TAG}"
docker tag "${ECR_REPO}:${IMAGE_TAG}" "${ECR_URI}:${ALIAS_TAG}"

echo "==> Push"
docker push "${ECR_URI}:${IMAGE_TAG}"
docker push "${ECR_URI}:${ALIAS_TAG}"

echo "==> Forçando novo deployment"
aws ecs update-service \
  --cluster "$CLUSTER" \
  --service "$SERVICE" \
  --force-new-deployment \
  --region "$REGION" >/dev/null

echo "==> Aguardando estabilizar..."
aws ecs wait services-stable \
  --cluster "$CLUSTER" \
  --services "$SERVICE" \
  --region "$REGION"

if [[ "$WAIT_TG_HEALTH" == "true" ]]; then
  TG_ARN="$(aws ecs describe-services \
    --cluster "$CLUSTER" \
    --services "$SERVICE" \
    --region "$REGION" \
    --query 'services[0].loadBalancers[0].targetGroupArn' \
    --output text || true)"

  if [[ -z "$TG_ARN" || "$TG_ARN" == "None" ]]; then
    echo "⚠️ Nenhum Target Group associado. Pulando health check."
  else
    echo "==> Monitorando Target Group até HEALTHY (timeout ${TIMEOUT}s)"
    START="$(date +%s)"
    while true; do
      STATES="$(aws elbv2 describe-target-health \
        --target-group-arn "$TG_ARN" \
        --region "$REGION" \
        --query 'TargetHealthDescriptions[].TargetHealth.State' \
        --output text || true)"
      echo "Estado atual: ${STATES:-<sem targets>}"
      if [[ -n "$STATES" ]] && [[ "$STATES" =~ ^(healthy[[:space:]]*)+$ ]]; then
        echo "✅ HEALTHY!"
        break
      fi
      (( $(date +%s) - START > TIMEOUT )) && { echo "❌ Timeout."; exit 1; }
      sleep "$SLEEP_SEC"
    done
  fi
fi

echo "==> Deploy concluído 🎉"
echo "    Imagem imutável: ${ECR_URI}:${IMAGE_TAG}"
echo "    Alias atualizado: ${ECR_URI}:${ALIAS_TAG}"
