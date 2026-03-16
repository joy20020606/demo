export type Hook = {
  id: string;
  name: string;
  category: string;
  description: string;
  repoUrl: string;
};

export const categories = [
  "CI/CD",
  "Serverless",
  "Webhooks",
  "Notifications",
  "DevOps",
  "Integrations",
] as const;

export const categoryColors: Record<string, string> = {
  "CI/CD": "bg-blue-100 text-blue-800",
  Serverless: "bg-purple-100 text-purple-800",
  Webhooks: "bg-green-100 text-green-800",
  Notifications: "bg-yellow-100 text-yellow-800",
  DevOps: "bg-red-100 text-red-800",
  Integrations: "bg-indigo-100 text-indigo-800",
};

export const hooks: Hook[] = [
  {
    id: "github-webhook-proxy",
    name: "GitHub Webhook Proxy",
    category: "Webhooks",
    description:
      "A lightweight proxy server that forwards GitHub webhook events to multiple endpoints with retry logic and event filtering.",
    repoUrl: "https://github.com/nicholasgasior/gogo",
  },
  {
    id: "lambda-github-deployer",
    name: "Lambda GitHub Deployer",
    category: "Serverless",
    description:
      "Automatically deploy AWS Lambda functions when code is pushed to GitHub. Supports multiple runtimes and environment configs.",
    repoUrl: "https://github.com/aws-samples/aws-lambda-deploy",
  },
  {
    id: "actions-status-discord",
    name: "Actions Status Discord",
    category: "CI/CD",
    description:
      "GitHub Action that sends rich build status notifications to Discord channels with customizable embed messages.",
    repoUrl: "https://github.com/sarisia/actions-status-discord",
  },
  {
    id: "vercel-deploy-hook",
    name: "Vercel Deploy Hook",
    category: "Serverless",
    description:
      "Trigger Vercel deployments via webhooks from any source. Supports branch filtering and deploy previews.",
    repoUrl: "https://github.com/vercel/vercel",
  },
  {
    id: "slack-github-action",
    name: "Slack GitHub Action",
    category: "Notifications",
    description:
      "Send custom Slack notifications from GitHub Actions workflows. Supports blocks, attachments, and threading.",
    repoUrl: "https://github.com/slackapi/slack-github-action",
  },
  {
    id: "n8n-webhook-node",
    name: "n8n Webhook Node",
    category: "Integrations",
    description:
      "Create custom webhook endpoints in n8n to receive data from external services and trigger automation workflows.",
    repoUrl: "https://github.com/n8n-io/n8n",
  },
  {
    id: "terraform-cloud-hooks",
    name: "Terraform Cloud Hooks",
    category: "DevOps",
    description:
      "Notification hooks for Terraform Cloud runs. Get alerts on plan, apply, and error states via Slack or email.",
    repoUrl: "https://github.com/hashicorp/terraform",
  },
  {
    id: "stripe-webhook-handler",
    name: "Stripe Webhook Handler",
    category: "Integrations",
    description:
      "A robust Stripe webhook handler with signature verification, event routing, and idempotent processing.",
    repoUrl: "https://github.com/stripe/stripe-node",
  },
  {
    id: "gitlab-ci-discord",
    name: "GitLab CI Discord Notifier",
    category: "CI/CD",
    description:
      "Send GitLab CI/CD pipeline status notifications to Discord. Supports merge request and pipeline events.",
    repoUrl: "https://github.com/DiscordHooks/gitlab-ci-discord-webhook",
  },
  {
    id: "netlify-plugin-webhooks",
    name: "Netlify Plugin Webhooks",
    category: "Serverless",
    description:
      "Netlify build plugin that sends webhook notifications on deploy success, failure, or preview builds.",
    repoUrl: "https://github.com/netlify/plugins",
  },
  {
    id: "kubernetes-webhook-admission",
    name: "K8s Admission Webhook",
    category: "DevOps",
    description:
      "Kubernetes admission webhook controller for validating and mutating resources before they are persisted.",
    repoUrl: "https://github.com/kubernetes/kubernetes",
  },
  {
    id: "zapier-webhook-trigger",
    name: "Zapier Webhook Trigger",
    category: "Integrations",
    description:
      "Catch webhooks from any service and connect them to 5,000+ apps via Zapier automation workflows.",
    repoUrl: "https://github.com/zapier/zapier-platform",
  },
];
