#!/bin/bash

# Script per gestire il setup del progetto con Docker

set -e

echo "?? StudySync Docker Setup"
echo "========================="

# Colori per output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Verifica se Docker è installato
if ! command -v docker &> /dev/null; then
    echo -e "${RED}? Docker non è installato. Installalo da https://www.docker.com/${NC}"
    exit 1
fi

# Verifica se Docker Compose è installato
if ! command -v docker-compose &> /dev/null; then
    echo -e "${RED}? Docker Compose non è installato.${NC}"
    exit 1
fi

echo -e "${GREEN}? Docker e Docker Compose trovati${NC}"

# Menu principale
echo ""
echo "Seleziona un'operazione:"
echo "1) Avvia i container (build + up)"
echo "2) Ferma i container"
echo "3) Ricostruisci i container"
echo "4) Applica le migrazioni al database"
echo "5) Visualizza i log"
echo "6) Pulisci tutto (remove containers, volumes, networks)"
echo "7) Esci"
echo ""
read -p "Inserisci il numero dell'operazione (1-7): " choice

case $choice in
    1)
        echo -e "${YELLOW}?? Avvio dei container...${NC}"
        docker-compose up -d --build
        echo ""
        echo -e "${GREEN}? Container avviati con successo!${NC}"
        echo ""
        echo "URL dell'applicazione:"
        echo "  - HTTP:  http://localhost:5000"
        echo "  - HTTPS: https://localhost:5001"
        echo ""
        echo "Credenziali MySQL:"
        echo "  - Host: mysql (da dentro Docker) / localhost:3306 (da host)"
        echo "  - User: root"
        echo "  - Password: password_super_segreta"
        echo "  - Database: studysync_db"
        ;;
    2)
        echo -e "${YELLOW}??  Fermo i container...${NC}"
        docker-compose down
        echo -e "${GREEN}? Container fermati${NC}"
        ;;
    3)
        echo -e "${YELLOW}?? Ricostruisco i container...${NC}"
        docker-compose up -d --build --force-recreate
        echo -e "${GREEN}? Container ricostruiti${NC}"
        ;;
    4)
        echo -e "${YELLOW}???  Applico le migrazioni al database...${NC}"
        docker-compose exec -T app dotnet ef database update -p Template -s Template.Web
        echo -e "${GREEN}? Migrazioni applicate${NC}"
        ;;
    5)
        echo -e "${YELLOW}?? Visualizzo i log dell'app...${NC}"
        docker-compose logs -f app
        ;;
    6)
        echo -e "${RED}??  ATTENZIONE: Questa operazione eliminerà tutto!${NC}"
        read -p "Sei sicuro? (s/n): " confirm
        if [ "$confirm" = "s" ] || [ "$confirm" = "S" ]; then
            echo -e "${YELLOW}???  Pulisco tutto...${NC}"
            docker-compose down -v
            echo -e "${GREEN}? Tutto pulito${NC}"
        else
            echo "Operazione annullata"
        fi
        ;;
    7)
        echo "Arrivederci!"
        exit 0
        ;;
    *)
        echo -e "${RED}? Opzione non valida${NC}"
        exit 1
        ;;
esac
