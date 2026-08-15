# Guía de usuario: el tablero de gitvisual

La pantalla de bienvenida (Welcome) es tu tablero multi-repo. Cada fila es un repositorio y
resume su estado de un vistazo. Esta guía explica cada indicador.

## Indicadores por repositorio

### Píldora oscura con número — cambios sin commitear

Cantidad de archivos modificados que aún no commiteaste (incluye archivos nuevos sin trackear).
Si no aparece, el repo está limpio.

Para ver el detalle: abrí el repo → vista de cambios locales → cada archivo con su diff;
desde ahí podés stagear, commitear o descartar.

### Texto `N↑ M↓` — divergencia con el remoto

- `N↑`: tenés N commits locales que todavía no pusheaste
- `M↓`: el remoto tiene M commits que todavía no trajiste (te falta pull/fetch)

Si no aparece, tu rama está sincronizada con su remoto.

### Píldora celeste con número — PRs/MRs abiertos

Cantidad de Pull Requests (GitHub) o Merge Requests (GitLab) abiertos en el repo remoto.
Requiere configurar el token del forge (ver abajo). Si no aparece: no hay token configurado,
el remoto no es GitHub/GitLab, o la consulta falló (nunca rompe las demás filas).

### Píldora con borde celeste e ícono de rama — ramas sin mergear

Cantidad de ramas locales que tienen commits que NO están en la rama principal
(se detecta vía `origin/HEAD`, o `main`/`master` local como fallback). Es la señal de
"acá hay trabajo empezado que nunca se integró". Si no aparece, todas tus ramas ya están
mergeadas (o el repo no tiene rama principal detectable).

### Ícono de advertencia naranja — repo inválido

La carpeta ya no existe o dejó de ser un repo git. Conviene quitarla de la lista.

## Toolbar de Welcome

- **Candado** — configurar tokens de forge (GitHub/GitLab). Ver siguiente sección.
- **Ícono de workspace** (con punto celeste cuando está activo) — filtra la lista para
  mostrar solo los repos del workspace activo (los que tenés abiertos en tabs).
- **Carpeta+ / escáner** — agregar grupo / reescanear el directorio de clonado por defecto.

## Configuración de tokens

Los conteos de PRs/MRs necesitan un Personal Access Token por plataforma:

| Forge | Dónde crearlo | Scope mínimo | Host en el popup |
|---|---|---|---|
| GitHub | github.com → Settings → Developer settings → Personal access tokens | `repo` (o `public_repo` si solo públicos) | `github.com` |
| GitLab | gitlab.com → Preferences → Access Tokens | `read_api` | `gitlab.com` o tu host self-managed |

**IMPORTANTE**: el Host debe escribirse completo (`github.com`, no `github`) — es la clave
con la que se relaciona el token con la URL del remoto de cada repo.

Los tokens se guardan en el **llavero de macOS** (nunca en archivos de texto plano).
Para eliminarlos manualmente:

```bash
security delete-generic-password -s "com.sourcegit.forge.github:github.com"
security delete-generic-password -s "com.sourcegit.forge.gitlab:gitlab.com"
```

Si un token expira o se revoca, el badge de PRs/MRs simplemente deja de aparecer.

## Refresco

- La lista se actualiza al abrir Welcome y al volver a la tab
- Los estados git (rama, divergencia, dirty, ramas sin mergear) tienen un throttle de 10s por repo
- Los conteos de forge se consultan en paralelo después del estado git
- Guardar un token fuerza un refresco inmediato de toda la lista
