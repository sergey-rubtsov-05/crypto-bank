variable "hcloud_token" {
  type      = string
  sensitive = true
}

variable "ssh_key_fingerprint" {
  type    = string
  default = "ce:8e:a4:76:0b:39:26:c6:28:79:f1:31:a8:91:3f:32"
}