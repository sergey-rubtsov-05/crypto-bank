terraform {
  required_version = ">= 1.6.6"
  required_providers {
    hcloud = {
      source  = "hetznercloud/hcloud"
      version = "1.44.1"
    }
  }
}