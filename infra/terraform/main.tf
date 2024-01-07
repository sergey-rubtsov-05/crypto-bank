data "hcloud_ssh_key" "ssh_key" {
  fingerprint = var.ssh_key_fingerprint
}

resource "hcloud_network" "network" {
  name     = "main_network"
  ip_range = "10.0.0.0/16"
}

resource "hcloud_network_subnet" "subnet" {
  type         = "cloud"
  network_id   = hcloud_network.network.id
  network_zone = "eu-central"
  ip_range     = "10.0.1.0/24"
}

locals {
  frontend_ip = "10.0.1.1"
  backend_ip  = "10.0.1.2"
  database_id = "10.0.1.3"
}

module "base_firewall" {
  source = "./modules/base_firewall"
}

module "frontend_server" {
  source = "./modules/frontend_server"

  name             = "frontend"
  network_id       = hcloud_network.network.id
  private_ip       = local.frontend_ip
  base_firewall_id = module.base_firewall.id
  ssh_keys         = [data.hcloud_ssh_key.ssh_key.name]
}

module "backend_server" {
  source = "./modules/backend_server"

  name             = "backend"
  network_id       = hcloud_network.network.id
  private_ip       = local.backend_ip
  base_firewall_id = module.base_firewall.id
  frontend_ip      = local.frontend_ip
  database_ip      = local.database_id
  ssh_keys         = [data.hcloud_ssh_key.ssh_key.name]
}

module "database_server" {
  source = "./modules/database_server"

  name             = "database"
  network_id       = hcloud_network.network.id
  private_ip       = local.database_id
  base_firewall_id = module.base_firewall.id
  backend_ip       = local.backend_ip
  ssh_keys         = [data.hcloud_ssh_key.ssh_key.name]
}