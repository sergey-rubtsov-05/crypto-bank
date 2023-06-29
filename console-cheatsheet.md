# docker commands

## build image. Current directory: "{repository}\src"
docker build -t CryptoBank -f .\CryptoBank.WebAPI\Dockerfile .

## run container
## -i - keep STDIN open even if not attached
## -t - allocate a pseudo-tty
## -d - detach STDIN
## -p - port mapping
## --rm - remove container after stopping
docker run --name CryptoBank -i -t -d -p 7180:80 CryptoBank

## list containers
docker ps

## The command fetches the logs of a container
docker logs {CONTAINER}

## The command executes a command in a running container
docker exec {CONTAINER} {COMMAND}
docker exec -it webapi bash