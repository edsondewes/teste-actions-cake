FROM nginx:1.17.5-alpine

# Configuração nginx
COPY standalone/nginx.conf /etc/nginx/nginx.conf

# Javascript gerado pelo build
COPY dist/* /usr/share/nginx/html/js/