help:
	@egrep "^#" Makefile

# target: docker-up|du                  - Start docker containers
du: docker-up
docker-up:
	docker compose up -d --build

# target: docker-down|dd                - Stop docker containers
dd: docker-down
docker-down:
	docker compose down

# target: docker-build|db               - Setup PHP dependencies
db: docker-build
docker-build: build-composer

build-composer:
	docker compose run --rm app sh -c "composer install"

# target: bash-app|ba                   - Connect to the app docker container
ba: bash-app
bash-app:
	docker compose run --rm app sh
#	docker compose exec app bash