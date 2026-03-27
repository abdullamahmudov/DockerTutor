# Resourses
https://skillbox.ru/media/code/n8n/


docker volume create n8n_data

docker run -it --rm \
 --name n8n \
 -p 5678:5678 \
 -v n8n_data:/home/node/.n8n \
 docker.n8n.io/n8nio/n8n



 docker volume create n8n_data

docker run -it --rm \
 --name n8n \
 -p 5678:5678 \
 -v n8n_data:/home/node/.n8n \
 docker.n8n.io/n8nio/n8n \
 start --tunnel


 docker volume create n8n_data

docker run -it --rm \
 --name n8n \
 -p 5678:5678 \
 -e DB_TYPE=postgresdb \
 -e DB_POSTGRESDB_DATABASE=<POSTGRES_DATABASE> \
 -e DB_POSTGRESDB_HOST=<POSTGRES_HOST> \
 -e DB_POSTGRESDB_PORT=<POSTGRES_PORT> \
 -e DB_POSTGRESDB_USER=<POSTGRES_USER> \
 -e DB_POSTGRESDB_SCHEMA=<POSTGRES_SCHEMA> \
 -e DB_POSTGRESDB_PASSWORD=<POSTGRES_PASSWORD> \
 -v n8n_data:/home/node/.n8n \
 docker.n8n.io/n8nio/n8n



 Stop the container and start it again:

1 Get the container ID:
docker ps -a
1 Stop the container with ID container_id:
docker stop [container_id]
1 Remove the container (this does not remove your user data) with ID container_id:
docker rm [container_id]
1 Start the new container:
docker run --name=[container_name] [options] -d docker.n8n.io/n8nio/n8n


docker run -it --rm \
 --name n8n \
 -p 5678:5678 \
 -e GENERIC_TIMEZONE="Europe/Berlin" \
 -e TZ="Europe/Berlin" \
 docker.n8n.io/n8nio/n8n



Container name : n8n
Ports :	5678
### Volume
Host path :	n8n_data
Container path :	/home/node/.n8n
### Variables
(выбрать можно с помощью подсказки здесь)
GENERIC_TIMEZONE: Europe/Moscow 
(ваша временная зона)
TZ : Europe/Moscow 
N8N_ENFORCE_SETTINGS_FILE_PERMISSIONS :	true
N8N_RUNNERS_ENABLED :	true