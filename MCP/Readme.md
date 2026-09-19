## MCP серверы для получения информации с веб-сайтов

Вот список популярных и полезных MCP серверов для работы с веб-контентом:

### 1. **Web Fetch / Website Information**
Серверы для извлечения контента с веб-страниц:
- **mcp-server-web** — универсальный сервер для парсинга веб-страниц
- **web-reader-mcp** — для чтения и извлечения основного содержимого статей

### 2. **SearXNG MCP Server**
Сервер для поиска через движок SearXNG (метаданные, заголовки, описание):
```bash
npx -y @anthropic/skills searxng
```

### 3. **Playwright MCP**
Для работы с браузерами и динамическим контентом:
- Позволяет открывать страницы, выполнять JS, делать скриншоты
- Используется для сайтов с JavaScript-рендерингом

### 4. **GitHub MCP Server**
Для получения информации из репозиториев GitHub:
```bash
npx -y @modelcontextprotocol/server-github
```

### 5. **Firestore/Google Services MCP**
Серверы от Google для доступа к данным и API.

---

### Где искать больше MCP серверов:

- **[mcpservers.org](https://mcpservers.org)** — реестр всех доступных MCP серверов с фильтрацией по категории (включая веб-инструменты)
- **[AWX MCP Servers](https://github.com/mcp-url-list/mcp-servers-list)** — коллекция MCP серверов с прямыми ссылками для установки

---

### Рекомендация:

Для базового извлечения информации с веб-сайтов (HTML, метаданные, текст) лучше всего подходит **web-reader-mcp** или любой сервер на базе **Playwright**, если нужен полноценный браузерный рендеринг.

### web-reader-mcp

```bash
docker pull ghcr.io/chauthan/web-reader-mcp:latest
```

```json
{
  "mcpServers": {
    "web-reader": {
      "command": "docker",
      "args": ["run", "-i", "--rm", "ghcr.io/chauthan/web-reader-mcp:latest"]
    }
  }
}
```

---

### playwright MCP

```bash
npm install -g @playwright/cli@latest
```

```bash
playwright-cli --help
```

```bash
playwright-cli install --skills
```

---