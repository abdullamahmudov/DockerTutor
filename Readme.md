# Docker

### RUN
```bash
docker-compose up --force-recreate
```

### Kubernetes
```bash
kubectl get all -n {namespace} # получение всей информации по namespace
```

```bash
kubectl get deployments -n {namespace} # получение всех deployments в namespace
```

```bash
kubectl get pods -n {namespace} # получение всех pods в namespace
```

```bash
kubectl apply -f {...-deployment.yaml} # сборка и запуск yaml
```

```bash
kubectl logs {pod} -n {namespace} # получение логов pod из namespace
```

```bash
kubectl describe pod {pod} -n {namespace} # получение информации о pod из namespace
```

```bash
kubectl create namespace {namespace} # добавление namespace
```

```bash
kubectl delete namespace {namespace} # удаление namespace
```

```bash
kubectl rollout restart deployment -n {namespace} # перезапуск deployment в namespace
```

```bash
kubectl get events --namespace={namespace} # получение событий в namespace
```

```bash
kubectl port-forward pgadmin-deployment-***-*** 5050:80 -n <namespace> # переадресация портов
```