docker service rm exporter
docker service create --replicas=4 --publish 8080:80 --limit-memory 5000MiB --mount type=bind,source=/root/screens,destination=/app/screens --name exporter trl