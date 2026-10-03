INSERT INTO users (id, name, email, role)
VALUES (
    '018f4c2a-0000-7000-8000-000000000001',
    'Administrator',
    'admin@urbanalert.com',
    'ADMIN'
)
ON CONFLICT DO NOTHING;
