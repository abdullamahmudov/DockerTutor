You can install Redis Insight using one of the options described below.

If you do not want to persist your Redis Insight data:
```bash
docker run -d --name redisinsight -p 5540:5540 redis/redisinsight:latest
```

If you want to persist your Redis Insight data, first attach the Docker volume to the `/data` path and then run the following command:
```bash
docker run -d --name redisinsight -p 5540:5540 -v redisinsight:/data redis/redisinsight:latest
```

If the previous command returns a permission error, ensure that the user with `ID = 1000` has the necessary permissions to access the volume provided (redisinsight in the command above).

Next, point your browser to `http://localhost:5540`.

Redis Insight also provides a health check endpoint at `http://localhost:5540/api/health/` to monitor the health of the running container.

