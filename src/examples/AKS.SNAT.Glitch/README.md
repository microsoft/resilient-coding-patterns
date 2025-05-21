

## Deploy the cluster

Run the provided terraform script to create the AKS cluster.

```bash
cd terraform
export ARM_SUBSCRIPTION_ID=<your-subscription-id>
terraform init
terraform apply
```

## Deploy the application

```bash
cd ../src
az aks get-credentials --resource-group <your-resource-group> --name <your-cluster-name>
kubectl apply -f aks-snat-glitch.yaml
```

## Check the cluster

After a while if you check the cluster load balancer you should see that there are SNAT issues.

## Fix the code

Modify the deployment to set the `FIXED` environment variable to `1` and redeploy the application.

```bash
kubectl apply -f aks-snat-glitch.yaml
```
