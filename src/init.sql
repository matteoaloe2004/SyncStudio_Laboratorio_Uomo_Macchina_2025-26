-- Create database if not exists
CREATE DATABASE IF NOT EXISTS studysync_db CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

-- Use the database
USE studysync_db;

-- Grant privileges to studysync_user
GRANT ALL PRIVILEGES ON studysync_db.* TO 'studysync_user'@'%' IDENTIFIED BY 'studysync_password';
GRANT ALL PRIVILEGES ON studysync_db.* TO 'studysync_user'@'localhost' IDENTIFIED BY 'studysync_password';

-- Flush privileges to make sure they're applied
FLUSH PRIVILEGES;
