# Validação Windows e PNG — modelo geral V1

Concluída em **17/11/2026**, entre 13h45 e 13h56 (São Paulo), com o desktop atual e SQL Server Express real. São dados sintéticos de demonstração, sem alunos identificados. Não foi necessário alterar a interface nesta rodada.

## Ambiente e integridade

- API em `http://127.0.0.1:5086`, banco exclusivo `TermometroGS_SOA_ModeloGeral_20261117_134420` e cinco migrações aplicadas.
- Desktop executado da cópia isolada `D:/codex-validacao-termometro-20261117-c0ce77f9efc54ab2885d0268d0cbb51c/desktop-visual-modelo-geral-v1`, com configuração pública apontando para essa API.
- Os quatro binários/runtime da cópia e de `executavel-desktop` foram comparados por SHA-256 com a saída compilada atual: [registro de integridade](../../../docs/resultados-testes-2026-11-17/desktop-binarios-modelo-geral-v1.tsv). A publicação do kit usa a URL pública padrão 5080.
- API reiniciada com caminho absoluto explícito do modelo. A árvore retornou 33,9917 → 34,0 para o CSV da hora 13/novembro; sem modelo, o CSV retornaria 35,3. [Conferência SQL/ML](../../../docs/resultados-testes-2026-11-17/sql-modelo-geral-v1.txt).

## Conferência pela interface real

| Verificação | Resultado e evidência |
|---|---|
| Modo local | Aviso de demonstração, origem dos registros e cinco salas de exemplo legíveis; nenhum registro local foi criado. [Tela](desktop-modo-local.jpg). |
| Volta à API | A atualização carregou cinco salas do banco e uma ação. Estado “API conectada — dados do servidor” confirmado. |
| Painel maximizado | Captura 1920 × 1032: cartões, comandos, legenda, horários, avisos e grades sem cortes observados. [Tela](desktop-sql-maximizado.jpg); [acessibilidade](desktop-sql-acessibilidade.txt). |
| Painel em janela | Captura 1222 × 882: aviso de histórico insuficiente quebra em linhas, grade rola e registro de ação/rodapé permanecem acessíveis. [Tela](desktop-sql-janela.jpg). |
| Previsão da Sala 3 | Rua 34,0 + delta 4,0 = sala 38,0, Cuidado extremo, `demonstracao=true`, histórico suficiente. A tela exibiu “Cuidado extremo às 15:49”, coerente com a [resposta da API](previsao-sala03.json). |
| Histórico insuficiente | Salas sem pares suficientes mostram explicitamente “Histórico insuficiente para ajustar a sala; previsão da rua”. Não aparece coeficiente inventado. |
| Mapa Agora | Vinte cadeiras C01–C20 e quatro sensores S1–S4, posições e códigos legíveis; S2, C16 e C20 em Frio azul. [Tela](mapa-agora-tela.jpg). |
| Mapa +1 h | Período carregado, temperaturas e distribuição mudam; códigos e posições permanecem. A aproximação proporcional ao tempo até o alvo está explicada. [Tela](mapa-1h-tela.jpg). |
| Rolagem da lista | C20 e todas as precauções alcançáveis e legíveis no fim da lista, em [Agora](mapa-agora-lista-fim.jpg) e [+1 h](mapa-1h-lista-fim.jpg). |
| Exportação real | Ambos os arquivos foram salvos pelo botão **Exportar PNG**, abertos e inspecionados integralmente; revisão independente também os aprovou. |

## PNGs exportados

| Arquivo | Dimensões | Bytes | SHA-256 |
|---|---:|---:|---|
| [Agora](mapa-agora-exportado.png) | 1144 × 929 | 66.712 | `e777be44cdb72d1f6ba7e65c74f40d04c449d0b6ffcb27242e6868ced2069c4c` |
| [+1 h](mapa-1h-exportado.png) | 1144 × 1000 | 71.818 | `5450e7b7a2681f14178ae2b4e87f2f20e896b7fad53e5b811e0dddaa0a6137c1` |

Título, período, legenda, 24 pontos, lista completa até C20, precauções e rodapé presentes nos dois PNGs, sem cortes ou sobreposições observados. As capturas de tela são JPEG; os arquivos de exportação são PNG reais.

## Encerramento e reprodução

O desktop de ensaio foi fechado. As APIs temporárias PID 18232 (5086, esta validação) e PID 12128 (5080, apresentação antiga das 09h45) tiveram identidade conferida e foram encerradas; todos os bancos foram preservados. A porta padrão ficou livre para criar uma cena recente com o iniciador do kit:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File ./ferramentas/abrir-apresentacao.ps1
```

O iniciador cria banco novo, mantém a chave temporária só em memória e abre o desktop. Confira Sala 3 e o mapa da Sala 7, alterne Agora/+1 h e exporte para nomes novos. Atualize os dados Demo após 15 minutos se a cena ficar antiga. O código do iniciador foi preservado nesta rodada; os binários que ele usa foram atualizados.

Cobertura: janela normal e maximizada, integração local, mapa e exportação. Não houve alteração de DPI do Windows, ensaio Mobile físico ou implantação em VM nesta rodada. O mapa futuro é aproximação, sem modelo individual treinado por cadeira; estes testes não medem precisão térmica em escolas reais.
