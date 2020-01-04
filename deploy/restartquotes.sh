#!/bin/bash
sudo systemctl daemon-reload
sudo systemctl restart kestrel-quotes.service
wget --retry-connrefused --waitretry=1 --read-timeout=20 --timeout=15 -t 0 --tries=inf -O- http://localhost:50001 > /dev/null
sudo nginx -s reload
