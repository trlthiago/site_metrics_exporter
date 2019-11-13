docker build -t trl .
docker stop exporter
docker rm exporter
docker run -d -p 8080:80 --name exporter trl