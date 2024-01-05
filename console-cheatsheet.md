# Docker commands

## Build image
working directory: `{repository}\backend\src`
- `-t` - Name and optionally a tag (format:  "name:tag")

`docker build -t crypto-bank -f .\CryptoBank.WebAPI\Dockerfile .`

## Update tag
`docker tag {SOURCE_TAG} {TARGET_TAG}`

## Push image
`docker push docker.io/sergeyrubtsov/crypto-bank:v0.0.5`

## Run container
- `-i` - keep STDIN open even if not attached
- `-t` - allocate a pseudo-tty
- `-d` - detach STDIN
- `-p` - port mapping
- `--rm` - remove container after stopping

`docker run --name CryptoBank -i -t -d -p 7180:80 CryptoBank`

## list containers
`docker ps`

## The command fetches the logs of a container
`docker logs {CONTAINER}`

## The command executes a command in a running container
`docker exec {CONTAINER} {COMMAND}`
`docker exec -it webapi bash`

## Run bitcoin node container
`docker run --name=bitcoind-node -d --mount type=bind,source=/d/bitcoin/testnet,target=/bitcoin/.bitcoin -p 18332:18332 kylemanna/bitcoind`
