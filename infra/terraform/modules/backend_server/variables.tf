variable "name" {
  type = string
}

variable "network_id" {
  type = number
}

variable "private_ip" {
  type = string
}

variable "base_firewall_id" {
  type = number
}

variable "frontend_ip" {
  type = string
}

variable "database_ip" {
  type = string
}

variable "ssh_keys" {
  type = list(string)
}
