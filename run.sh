#!/bin/bash
docker stop quotes
docker rm quotes
docker run -d -p 8123:80 --name quotes quotes