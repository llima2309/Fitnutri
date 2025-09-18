#!/usr/bin/env bash
set -euo pipefail

############################################
# Config Fitnutri - ajuste conforme o seu #
############################################
REGION="${REGION:-us-east-1}"

# Use os ARNs reais do seu ambiente:
CLUSTER="${CLUSTER:-arn:aws:ecs:us-east-1:763548578114:cluster/fitnutri-cluster}"
SERVICE="${SERVICE:-arn:aws:ecs:us-east-1:763548578114:service/fitnutri-cluster/fitnutri-api-task-service-4kszczgw}"

# Repositório no ECR
ECR_REPO="${ECR_REPO:-fitnutri-api}"

# Tag imutável (padrão data+gitsha|local). Pode sobrescrever passando primeiro argumento.
IMAGE_TAG="${1:-$(date +%Y%m%d-%H%M)-$(git rev-parse --short HEAD 2>/dev/null || echo local)}"

# Alias mutável apontado pela Task Definition (ex.: prod)
ALIAS_TAG="${ALIAS_TAG:-prod}"

# Build
PLATFORM="${PLATFORM:-linux/amd64}"
DOCKERFILE="${DOCKERFILE:-DockerfileApi}"
BUILD_CONTEXT="${BUILD_CONTEXT:-.}"

# Health check
WAIT_TG_HEALTH="${WAIT_TG_HEALTH:-true}"   # true/false
TIMEOUT="${TIMEOUT:-300}"                  # segundos
SLEEP_SEC="${SLEEP_SEC:-10}"
############################################

echo "==> Deploy Fitnutri API"
echo "Região...............: $REGION"
echo "Cluster..............: $CLUSTER"
echo "Service..............: $SERVICE"
echo "ECR Repo.............: $ECR_REPO"
echo "Tag imutável.........: $IMAGE_TAG"
echo "Alias (Task aponta)..: $ALIAS_TAG"
echo "Plataforma...........: $PLATFORM"

# Descobre conta e monta URI do ECR
ACCOUNT_ID="$(aws sts get-caller-identity --query Account --output text)"
ECR_URI="${ACCOUNT_ID}.dkr.ecr.${REGION}.amazonaws.com/${ECR_REPO}"

# Valida pré-requisitos básicos
for cmd in aws docker; do
  command -v "$cmd" >/dev/null 2>&1 || { echo "❌ Comando '$cmd' não encontrado."; exit 1; }
done

# Valida Cluster ativo
STATUS_CLUSTER="$(aws ecs describe-clusters --clusters "$CLUSTER" --region "$REGION" --query 'clusters[0].status' --output text || true)"
if [[ "$STATUS_CLUSTER" != "ACTIVE" ]]; then
  echo "❌ Cluster não encontrado/ativo: $CLUSTER"
  exit 1
fi

# Valida Service
FAILURES="$(aws ecs describe-services --cluster "$CLUSTER" --services "$SERVICE" --region "$REGION" --query 'failures' --output json)"
if echo "$FAILURES" | grep -q '"reason"'; then
  echo "❌ Service não encontrado no cluster. Detalhes:"
  echo "$FAILURES"
  exit 1
fi

echo "==> Login no ECR"
aws ecr get-login-password --region "$REGION" \
  | docker login --username AWS --password-stdin "${ACCOUNT_ID}.dkr.ecr.${REGION}.amazonaws.com"

echo "==> Build da imagem ($PLATFORM)"
# buildx lida melhor com multiplataforma no Mac/Windows
docker buildx build \
  --platform "$PLATFORM" \
  -f "$DOCKERFILE" \
  -t "${ECR_REPO}:${IMAGE_TAG}" \
  "$BUILD_CONTEXT" \
  --load

echo "==> Tagging para o ECR"
docker tag "${ECR_REPO}:${IMAGE_TAG}" "${ECR_URI}:${IMAGE_TAG}"
docker tag "${ECR_REPO}:${IMAGE_TAG}" "${ECR_URI}:${ALIAS_TAG}"

echo "==> Push das imagens"
docker push "${ECR_URI}:${IMAGE_TAG}"
docker push "${ECR_URI}:${ALIAS_TAG}"

echo "==> Forçando novo deployment no ECS"
aws ecs update-service \
  --cluster "$CLUSTER" \
  --service "$SERVICE" \
  --force-new-deployment \
  --region "$REGION" >/dev/null

echo "==> Aguardando ECS estabilizar..."
aws ecs wait services-stable \
  --cluster "$CLUSTER" \
  --services "$SERVICE" \
  --region "$REGION"

# Health check no Target Group (opcional)
if [[ "$WAIT_TG_HEALTH" == "true" ]]; then
  TG_ARN="$(aws ecs describe-services \
    --cluster "$CLUSTER" \
    --services "$SERVICE" \
    --region "$REGION" \
    --query 'services[0].loadBalancers[0].targetGroupArn' \
    --output text || true)"

  if [[ -z "$TG_ARN" || "$TG_ARN" == "None" ]]; then
    echo "⚠️ Nenhum Target Group associado. Pulando health check ALB."
  else
    echo "==> Monitorando Target Group até HEALTHY (timeout ${TIMEOUT}s)"
    START="$(date +%s)"
    while true; do
      STATES="$(aws elbv2 describe-target-health \
        --target-group-arn "$TG_ARN" \
        --region "$REGION" \
        --query 'TargetHealthDescriptions[].TargetHealth.State' \
        --output text || true)"

      echo "Estado atual das targets: ${STATES:-<sem targets>}"

      if [[ -n "$STATES" ]] && [[ "$STATES" =~ ^(healthy[[:space:]]*)+$ ]]; then
        echo "✅ Todas as targets HEALTHY no ALB!"
        break
      fi

      NOW="$(date +%s)"
      ELAPSED=$(( NOW - START ))
      if (( ELAPSED > TIMEOUT )); then
        echo "❌ Timeout: targets não ficaram healthy em ${TIMEOUT}s."
        exit 1
      fi
      sleep "$SLEEP_SEC"
    done
  fi
fi

echo "==> Deploy concluído com sucesso 🎉"
echo "    Imagem imutável: ${ECR_URI}:${IMAGE_TAG}"
echo "    Alias atualizado: ${ECR_URI}:${ALIAS_TAG}"
