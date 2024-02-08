help:
	@egrep "^#" Makefile

# target: up                                     - Start project in docker container
up: 
	docker compose up -d

# target: down                                     - Start project in docker container
down: 
	docker compose down

# target: pu|prod-update                         - Update repo and start docker container
pu: prod-update
prod-update:
	git pull
	docker compose up -d --no-deps --build xebot
