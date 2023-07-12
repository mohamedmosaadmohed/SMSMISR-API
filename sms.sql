CREATE TABLE SmsMessages (
  Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  MobileNumbers NVARCHAR(MAX) NOT NULL,
  Message NVARCHAR(MAX) NOT NULL,
  Language NVARCHAR(50) NOT NULL,
  Cost DECIMAL(18, 2) NOT NULL,
  SentDateTime DATETIME2(7) NOT NULL
);
CREATE PROCEDURE InsertSmsMessage
  @MobileNumbers NVARCHAR(MAX),
  @Message NVARCHAR(MAX),
  @Language NVARCHAR(50),
  @Cost DECIMAL(18, 2),
  @SentDateTime DATETIME2(7)
AS
BEGIN
  INSERT INTO SmsMessages (MobileNumbers, Message, Language, Cost, SentDateTime)
  VALUES (@MobileNumbers, @Message, @Language, @Cost, @SentDateTime);
END

select *from SmsMessages