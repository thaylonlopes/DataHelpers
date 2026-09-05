# ADR 002: Decisões Arquiteturais do Pacote TL.Caching.Helpers

---

## Contexto

Aplicações de alto volume de tráfego necessitam de estratégias híbridas de cache combinando velocidade em memória local com consistência distribuída em clusters Redis. Em momentos de alta concorrência e expiração simultânea de chaves quentes, microsserviços sofrem com o fenômeno de *Cache Stampede* (ou *Thundering Herd*), onde centenas de threads tentam recalcular a mesma informação pesada e sobrecarregam o banco de dados.

O pacote `TL.Caching.Helpers` foi desenvolvido para resolver esses desafios através de uma camada híbrida multinível (L1/L2), distributed locking defensivo e otimização agressiva de banda de rede via compressão GZip.

---

## Decisões Arquiteturais

### 1. Abstração Unificada (`ICacheService`)
- Define um contrato desacoplado de provedor para leitura, escrita com expiração absoluta/deslizante, remoção e expiração condicional.
- Permite que o código de negócio dependa de uma abstração agnóstica sem acoplamento direto com a biblioteca cliente do Redis.

### 2. Proteção contra Cache Stampede via RedLock (`LockedCacheService`)
- Implementa o padrão *Double-Checked Locking* distribuído com `GetOrSetWithLockAsync<T>` utilizando o algoritmo RedLock (`RedLockNet`):
  1. Realiza busca inicial rápida no cache (caminho otimista em caso de cache hit).
  2. Em caso de miss, solicita lock distribuído exclusivo com chave `lock:stampede:{key}` e timeout com jitter defensivo.
  3. Ao adquirir o lock, reavalia o cache para verificar se outra instância concorrente já repovoou o valor.
  4. Somente se o cache continuar vazio, executa a factory do banco de dados, grava o cache e libera o lock.

### 3. Compressão Transparente GZip no Redis
- Objetos serializados que excedem o limiar de transferência são compactados de forma transparente via `CompressionHelper` (`GZipStream`).
- Reduz drasticamente o throughput de rede e o custo de memória RAM no cluster Redis.

### 4. Invalidação Granular por Regiões e Tenants
- Agrupa chaves sob namespaces lógicos (`region`), permitindo expirar atomicamente todas as chaves associadas a um tenant específico sem executar operações bloqueantes de varredura global (`KEYS *`).

---

## Consequências e Trade-offs

- **Resiliência Máxima:** Proteção completa contra picos de carga decorrentes de expiração simultânea de dados quentes.
- **Eficiência de Rede:** Compressão GZip reduz volume de dados trafegados entre aplicação e Redis.
- **Trade-off de Latência:** A aquisição de locks distribuídos no caminho de miss adiciona alguns milissegundos para a primeira requisição que recalcula o dado, protegendo o banco contra sobrecarga sistêmica.

