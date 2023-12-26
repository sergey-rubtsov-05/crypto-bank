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

resource "hcloud_server" "api_server" {
  name        = "api1"
  location    = "nbg1"
  server_type = "cx11"
  image       = "ubuntu-22.04"
  network {
    network_id = hcloud_network.network.id
  }
}