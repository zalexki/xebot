help:
	@egrep "^#" Makefile

# target: up                                     - Start rabbit in docker container
up: 
	docker-compose up -d

# target: pu|prod-update                          - Start dotnet worker and rabbit in docker containers
po: prod-update
prod-update:
	git pull
	docker compose -d --no-deps --build xebot