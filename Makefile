help:
	@egrep "^#" Makefile

# target: up                                     - Start project in docker container
up: 
	docker-compose up -d

# target: pu|prod-update                         - Update repo and start docker container
pu: prod-update
prod-update:
	git pull
	docker compose up -d --no-deps --build xebot