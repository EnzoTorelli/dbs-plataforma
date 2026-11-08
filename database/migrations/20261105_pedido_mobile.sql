-- Rode DEPOIS de banco_postgres.sql e de 20261018_create_vendedor_app.sql
-- Seguro para rodar mais de uma vez (IF NOT EXISTS).

ALTER TABLE pedido
    ADD COLUMN IF NOT EXISTS id_vendedor      INT REFERENCES vendedor_app(id),
    ADD COLUMN IF NOT EXISTS forma_pagamento  VARCHAR(30),
    ADD COLUMN IF NOT EXISTS parcelas         INT;

CREATE INDEX IF NOT EXISTS ix_pedido_vendedor_data
    ON pedido (id_vendedor, data_pedido);

-- Descomente SOMENTE se o mobile for cadastrar CNPJ (18 caracteres: 00.000.000/0000-00).
-- Atenção: a validação de CPF em Validacao.cs rejeitaria CNPJ no cadastro web.
-- ALTER TABLE cliente ALTER COLUMN cpf TYPE VARCHAR(18);
