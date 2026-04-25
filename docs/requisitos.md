# Requisitos

## Usuário

### Cadastrar perfil
- Como usuário, quero cadastrar meu perfil, para acessar o sistema

**Critérios de aceitação:**
- O sistema deve solicitar nome, e-mail, CPF, senha e telefone
- O e-mail e CPF devem ser únicos no sistema
- A senha deve ter no mínimo 8 caracteres
- Após o cadastro, o usuário deve receber um e-mail de confirmação
- Campos obrigatórios não preenchidos devem exibir mensagem de erro

---

### Fazer login
- Como usuário, quero fazer login, para acessar minha conta

**Critérios de aceitação:**
- O sistema deve aceitar e-mail e senha como credenciais
- Credenciais inválidas devem exibir mensagem de erro
- Após login bem-sucedido, o usuário deve ser redirecionado para a tela inicial
- O sistema deve bloquear o acesso após 5 tentativas incorretas

---

### Visualizar lista de viagens disponíveis
- Como usuário, quero visualizar a lista de viagens disponíveis, para escolher uma viagem

**Critérios de aceitação:**
- A lista deve exibir origem, destino, data, horário e preço
- Apenas viagens com status ativo devem ser exibidas
- A lista deve ser ordenada por data e horário

---

### Visualizar assentos disponíveis
- Como usuário, quero visualizar os assentos disponíveis, para escolher onde sentar

**Critérios de aceitação:**
- O sistema deve exibir um mapa visual dos assentos da van
- Assentos ocupados ou reservados devem estar indisponíveis para seleção
- O usuário deve conseguir selecionar apenas um assento por reserva

---

### Pagar reserva via Pix
- Como usuário, quero pagar a reserva via Pix, para concluir a compra

**Critérios de aceitação:**
- O sistema deve gerar um QR Code e chave Pix para pagamento
- O assento deve ser reservado temporariamente por 10 minutos aguardando o pagamento
- Após confirmação do pagamento, a reserva deve ser confirmada automaticamente
- O usuário deve receber confirmação por e-mail após o pagamento

---

### Visualizar minhas reservas
- Como usuário, quero visualizar minhas reservas, para acompanhar minhas viagens

**Critérios de aceitação:**
- O sistema deve listar todas as reservas do usuário (ativas, concluídas e canceladas)
- Cada reserva deve exibir origem, destino, data, horário, assento e status
- O usuário deve conseguir acessar o comprovante de cada reserva

---

### Pesquisar viagens
- Como usuário, quero pesquisar viagens, para encontrar uma viagem específica

**Critérios de aceitação:**
- O sistema deve permitir filtrar por origem, destino e data
- Caso nenhuma viagem seja encontrada, deve exibir mensagem informativa
- Os resultados devem ser exibidos em ordem de data e horário

---

### Receber confirmação por e-mail após a compra
- Como usuário, quero receber confirmação por e-mail após a compra, para ter um comprovante

**Critérios de aceitação:**
- O e-mail deve ser enviado automaticamente após a confirmação do pagamento
- O e-mail deve conter origem, destino, data, horário, número do assento e valor pago
- O e-mail deve conter um comprovante ou código da reserva

---

### Solicitar reembolso
- Como usuário, quero solicitar reembolso até 48h antes da viagem, para recuperar meu dinheiro ou crédito

**Critérios de aceitação:**
- O sistema deve permitir cancelamento apenas com mais de 48h de antecedência
- O reembolso pode ser em dinheiro ou crédito na plataforma, conforme escolha do usuário
- Após o cancelamento, o assento deve ser liberado automaticamente
- O usuário deve receber confirmação do cancelamento por e-mail

---

### Receber reembolso por viagem cancelada
- Como usuário, quero receber reembolso caso a viagem não tenha passageiros suficientes, para não ter prejuízo

**Critérios de aceitação:**
- O sistema deve notificar o usuário por e-mail em caso de cancelamento da viagem
- O reembolso deve ser processado automaticamente
- O usuário deve receber confirmação do reembolso por e-mail

---

### Receber e-mail quando pagamento não for efetuado
- Como usuário, quero receber um e-mail quando o pagamento não for efetuado, para tentar novamente

**Critérios de aceitação:**
- O e-mail deve ser enviado automaticamente após o tempo limite de 10 minutos expirar
- O e-mail deve conter um link ou instrução para realizar nova tentativa de reserva
- O assento deve ser liberado após o envio do e-mail

---

### Visualizar fotos da van
- Como usuário, quero visualizar fotos da van, para avaliar a qualidade antes de reservar

**Critérios de aceitação:**
- O sistema deve exibir ao menos uma foto da van na página da viagem
- As fotos devem ser cadastradas pelo administrador
- As fotos devem ser exibidas em boa resolução

---

## Administrador

### Definir tempo limite para pagamento
- Como administrador, quero definir um tempo limite de 10 minutos para pagamento, para liberar o assento caso o pagamento não seja efetuado

**Critérios de aceitação:**
- O tempo limite padrão deve ser de 10 minutos
- Após o tempo expirar, o assento deve ser liberado automaticamente
- O sistema deve notificar o usuário por e-mail sobre o tempo expirado

---

### Cadastrar novas viagens
- Como administrador, quero cadastrar novas viagens, para disponibilizá-las aos usuários

**Critérios de aceitação:**
- O sistema deve solicitar origem, destino, data, horário, van e preço
- Não deve ser possível cadastrar viagens com datas no passado
- A viagem deve ficar disponível para os usuários imediatamente após o cadastro
- O sistema deve impedir o cadastro de viagens conflitantes para a mesma van

---

### Pesquisar usuários cadastrados
- Como administrador, quero pesquisar usuários cadastrados, para facilitar o gerenciamento

**Critérios de aceitação:**
- O sistema deve permitir busca por nome, e-mail ou CPF
- Os resultados devem exibir nome, e-mail, telefone e status da conta
- Caso nenhum usuário seja encontrado, deve exibir mensagem informativa

---

### Cadastrar vans
- Como administrador, quero cadastrar vans, para utilizá-las nas viagens

**Critérios de aceitação:**
- O sistema deve solicitar placa, modelo, ano, capacidade de passageiros e fotos
- A placa deve ser única no sistema
- A van deve ficar disponível para uso em viagens após o cadastro

---

### Gerenciar usuários
- Como administrador, quero gerenciar usuários, para manter o controle do sistema

**Critérios de aceitação:**
- O administrador deve poder ativar, desativar e editar usuários
- O administrador deve poder alterar o perfil do usuário (passageiro, motorista)
- Ações realizadas devem ser registradas em log

---

### Impedir viagens conflitantes
- Como administrador, quero impedir o cadastro de viagens conflitantes para a mesma van, para evitar sobreposição de horários

**Critérios de aceitação:**
- O sistema deve verificar se a van já possui viagem cadastrada no mesmo horário
- Caso haja conflito, o sistema deve exibir mensagem de erro e bloquear o cadastro
- O sistema deve considerar o tempo estimado da viagem para verificar conflitos

---

### Definir ponto de encontro
- Como administrador, quero definir um ponto de encontro com foto ou link do mapa, para facilitar o embarque

**Critérios de aceitação:**
- O sistema deve permitir cadastrar um endereço, foto e/ou link do Google Maps para o ponto de encontro
- O ponto de encontro deve ser exibido para o passageiro na reserva e no e-mail de confirmação

---

## Motorista

### Visualizar lista de passageiros
- Como motorista, quero visualizar a lista de passageiros, para organizar o embarque

**Critérios de aceitação:**
- A lista deve exibir nome, assento e status de pagamento de cada passageiro
- A lista deve estar disponível apenas para a viagem do motorista
- O motorista deve conseguir marcar o embarque de cada passageiro

---

### Visualizar dashboard de ocupação
- Como motorista, quero visualizar um dashboard de ocupação das viagens, para acompanhar a lotação

**Critérios de aceitação:**
- O dashboard deve exibir o número de assentos ocupados e disponíveis por viagem
- As informações devem ser atualizadas em tempo real
- O motorista deve visualizar apenas suas próprias viagens

---

### Cancelar viagem
- Como motorista, quero cancelar uma viagem, para lidar com baixa ocupação ou imprevistos

**Critérios de aceitação:**
- O motorista deve informar o motivo do cancelamento
- Todos os passageiros devem ser notificados por e-mail automaticamente
- O reembolso deve ser processado automaticamente para todos os passageiros

---

### Visualizar agenda de viagens
- Como motorista, quero visualizar minha agenda de viagens, para me organizar

**Critérios de aceitação:**
- A agenda deve exibir todas as viagens do motorista ordenadas por data
- Deve ser possível visualizar detalhes de cada viagem
- A agenda deve diferenciar visualmente viagens futuras, em andamento e concluídas

---

### Visualizar ocupação da viagem
- Como motorista, quero visualizar a ocupação da viagem, para acompanhar o número de passageiros

**Critérios de aceitação:**
- O sistema deve exibir o total de assentos, ocupados e disponíveis
- As informações devem ser atualizadas conforme novas reservas são realizadas

---

### Alterar status da viagem
- Como motorista, quero alterar o status da viagem, para atualizar seu andamento

**Critérios de aceitação:**
- Os status disponíveis devem ser: agendada, em andamento, concluída e cancelada
- A alteração de status deve ser registrada com data e hora
- Passageiros devem ser notificados em caso de mudança para cancelada

---

### Registrar ocorrências
- Como motorista, quero registrar ocorrências durante a viagem, para documentar problemas

**Critérios de aceitação:**
- O motorista deve poder descrever a ocorrência e anexar foto se necessário
- A ocorrência deve ser registrada com data, hora e viagem vinculada
- O administrador deve ter acesso às ocorrências registradas

---

## Administrador ou Motorista

### Visualizar relatórios de faturamento
- Como administrador ou motorista, quero visualizar relatórios de faturamento, para análise financeira

**Critérios de aceitação:**
- O relatório deve exibir total arrecadado por período, viagem e van
- O motorista deve visualizar apenas o faturamento das suas próprias viagens
- O administrador deve visualizar o faturamento geral
- O relatório deve poder ser exportado em PDF ou Excel

---

## Passageiro

### CPF mascarado nos relatórios
- Como passageiro, quero ter meu CPF mascarado nos relatórios, para proteger meus dados

**Critérios de aceitação:**
- O CPF deve ser exibido no formato ***.XXX.XXX-** nos relatórios
- O CPF completo deve ser acessível apenas para o administrador em tela específica
- Nenhum relatório exportado deve conter o CPF completo do passageiro
