-- Script convertido de MySQL (XAMPP) para PostgreSQL (Supabase)
-- Não precisa de CREATE DATABASE / USE: o Supabase já cria o banco "postgres" pra você.
-- Não precisa de SET FOREIGN_KEY_CHECKS: a ordem das tabelas abaixo já respeita as dependências.

DROP TABLE IF EXISTS item_pedido;
DROP TABLE IF EXISTS pedido;
DROP TABLE IF EXISTS produto;
DROP TABLE IF EXISTS empresa;
DROP TABLE IF EXISTS cliente;
DROP TABLE IF EXISTS categoria;

CREATE TABLE categoria (
  id   SERIAL PRIMARY KEY,
  nome VARCHAR(50) NOT NULL
);
INSERT INTO categoria (id, nome) VALUES
  (1,'RAM'),(2,'SSD'),(3,'Processador'),(4,'Placa de Video');

CREATE TABLE cliente (
  id             SERIAL PRIMARY KEY,
  nome           VARCHAR(100) NOT NULL,
  cpf            VARCHAR(14),
  email          VARCHAR(100),
  telefone       VARCHAR(20),
  data_cadastro  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
INSERT INTO cliente (id, nome, cpf, email, telefone, data_cadastro) VALUES
  (1,'João Silva','123.456.789-00','joao@email.com','(19)98888-8888','2026-04-13 22:02:32');

CREATE TABLE empresa (
  id             SERIAL PRIMARY KEY,
  nome           VARCHAR(100) NOT NULL,
  cnpj           VARCHAR(20),
  email          VARCHAR(100),
  telefone       VARCHAR(20),
  data_cadastro  TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);
INSERT INTO empresa (id, nome, cnpj, email, telefone, data_cadastro) VALUES
  (1,'TechPoint Informatica','00.000.000/0001-00','contato@techpoint.com','(19)99999-9999','2026-04-13 22:02:31'),
  (2,'TechPoint Informatica','12.345.678/0001-99','contato@techpoint.com','(19)99999-9999','2026-04-13 22:05:03');

-- Tabela "pedido": adicionei a coluna "valor", que não existia no banco.sql
-- original mas é usada pelo PedidoRepository.cs (p.valor AS valor_total).
CREATE TABLE pedido (
  id           SERIAL PRIMARY KEY,
  id_cliente   INT REFERENCES cliente(id),
  data_pedido  TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  status       VARCHAR(50) DEFAULT 'Pendente',
  valor        DECIMAL(10,2) DEFAULT 0
);

CREATE TABLE produto (
  id            SERIAL PRIMARY KEY,
  nome          VARCHAR(100) NOT NULL,
  descricao     TEXT,
  preco         DECIMAL(10,2) NOT NULL,
  estoque       INT DEFAULT 0,
  id_categoria  INT REFERENCES categoria(id)
);
INSERT INTO produto (id, nome, descricao, preco, estoque, id_categoria) VALUES
  (1,'Memoria RAM 8GB','DDR4',199.90,10,1),
  (2,'RTX 3060','Placa de video NVIDIA',2500.00,5,1),
  (4,'Desktop Dell Pro Micro','Windows 11 Home / Intel Core i5 14500T / 8 GB DDR5 / 256GB SSD',6899.00,5,2),
  (6,'Mouse Gamer REDRAGON','7 Botoes',158.99,5,3);

CREATE TABLE item_pedido (
  id              SERIAL PRIMARY KEY,
  id_pedido       INT REFERENCES pedido(id),
  id_produto      INT REFERENCES produto(id),
  quantidade      INT NOT NULL,
  preco_unitario  DECIMAL(10,2)
);

-- Como as colunas SERIAL já geraram valores 1,2,3... ao inserir os IDs manuais acima
-- (ex: categoria 1-4, produto 1,2,4,6), a sequence interna do Postgres fica desatualizada.
-- Isso ajusta as sequences para o próximo INSERT sem ID não colidir com os que já existem:
SELECT setval(pg_get_serial_sequence('categoria','id'), COALESCE((SELECT MAX(id) FROM categoria), 1));
SELECT setval(pg_get_serial_sequence('cliente','id'),   COALESCE((SELECT MAX(id) FROM cliente), 1));
SELECT setval(pg_get_serial_sequence('empresa','id'),   COALESCE((SELECT MAX(id) FROM empresa), 1));
SELECT setval(pg_get_serial_sequence('produto','id'),   COALESCE((SELECT MAX(id) FROM produto), 1));
