-- create logins
CREATE LOGIN TestLogin WITH PASSWORD = 'ABC123'
GO

-- create users
CREATE USER TestUser FOR LOGIN TestLogin
GO
