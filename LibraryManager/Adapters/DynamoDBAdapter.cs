using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using LibraryManager.Interfaces;
using LibraryManager.Models;
using LibraryManager.Utilities;

namespace LibraryManager.Adapters
{
    public class DynamoDBAdapter : IDynamoDBAdapter
    {
        private AmazonDynamoDBClient dynamoDBClient;

        public DynamoDBAdapter(AmazonDynamoDBClient dynamoDBClient)
        {
            this.dynamoDBClient = dynamoDBClient;
        }

        public async Task<Book> GetBookById(string id)
        {
            try
            {
                QueryResponse result = await this.dynamoDBClient.QueryAsync(new QueryRequest()
                {
                    TableName = Constants.BOOK_TABLE_NAME,
                    KeyConditionExpression = "Id = :k",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {
                            ":k", new AttributeValue { S = id }
                        }
                    }
                }).ConfigureAwait(false);

                if (result.Items != null && result.Items.Count > 0)
                {
                    var item = result.Items[0];

                    var book = new Book()
                    {
                        Id = item["Id"].S,
                        Title = item["Title"].S,
                        Author = item["Author"].S,
                        Available = item["Available"].S,
                    };

                    return book;
                }

                return null;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<Transaction> GetTransactionById(string id)
        {
            try
            {
                QueryResponse result = await this.dynamoDBClient.QueryAsync(new QueryRequest()
                {
                    TableName = Constants.TRANSACTION_TABLE_NAME,
                    KeyConditionExpression = "Id = :k",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {
                            ":k", new AttributeValue { S = id }
                        }
                    }
                }).ConfigureAwait(false);

                if (result.Items != null && result.Items.Count > 0)
                {
                    var item = result.Items[0];

                    var transaction = new Transaction()
                    {
                        Id = item["Id"].S,
                        BookId = item["BookId"].S,
                        StudentId = item["StudentId"].S,
                        Active = item["Active"].S,
                    };

                    return transaction;
                }

                return null;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<Student> GetStudentById(string id)
        {
            try
            {
                QueryResponse result = await this.dynamoDBClient.QueryAsync(new QueryRequest()
                {
                    TableName = Constants.STUDENT_TABLE_NAME,
                    KeyConditionExpression = "Id = :k",
                    ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                    {
                        {
                            ":k", new AttributeValue { S = id }
                        }
                    }
                }).ConfigureAwait(false);

                if (result.Items != null && result.Items.Count > 0)
                {
                    var item = result.Items[0];
                    var student = new Student()
                    {
                        Id = item["Id"].S,
                        FirstName = item["FirstName"].S
                    };
                    return student;
                }
                return null;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<Profile> GetProfileByStudentId(string id)
        {
            try
            {
                var profile = new Profile();
                var student = await GetStudentById(id);

                profile.StudentId = student.Id;
                profile.FirstName = student.FirstName;

                var activeBooks = new List<Book>();
                var previousBooks = new List<Book>();
                var activeTransactions = await GetTransactionsByStudentId(id, "true");
                foreach (var transaction in activeTransactions)
                {
                    var book = await GetBookById(transaction.BookId);
                    activeBooks.Add(book);
                }
                profile.CurrentBooks = activeBooks;

                var previousTransactions = await GetTransactionsByStudentId(id, "false");
                foreach (var transaction in previousTransactions)
                {
                    var book = await GetBookById(transaction.BookId);
                    previousBooks.Add(book);
                }
                profile.PreviousBooks = previousBooks;

                return profile;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<List<Book>> GetBooksByAvailability(string available)
        {
            try
            {
                Dictionary<string, Condition> conditions = new Dictionary<string, Condition>();
                Condition titleCondition = new Condition();
                titleCondition.ComparisonOperator = ComparisonOperator.CONTAINS;
                titleCondition.AttributeValueList.Add(new AttributeValue { S = available });
                conditions["Available"] = titleCondition;
                Dictionary<string, AttributeValue> startKey = null;
                var result = await this.dynamoDBClient.ScanAsync(new ScanRequest()
                {
                    TableName = Constants.BOOK_TABLE_NAME,
                    ExclusiveStartKey = startKey,
                    ScanFilter = conditions
                }).ConfigureAwait(false);
                
                if (result.Items != null && result.Items.Count > 0)
                {
                    var books = new List<Book>();
                    foreach (var item in result.Items)
                    {
                        var book = new Book()
                        {
                            Id = item["Id"].S,
                            Title = item["Title"].S,
                            Author = item["Author"].S,
                            Available = item["Available"].S,
                        };
                        books.Add(book);
                    }
                    return books;
                }
                return null;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<List<Transaction>> GetTransactionsByStatus(string status)
        {
            try
            {
                Dictionary<string, Condition> conditions = new Dictionary<string, Condition>();
                Condition titleCondition = new Condition();
                titleCondition.ComparisonOperator = ComparisonOperator.CONTAINS;
                titleCondition.AttributeValueList.Add(new AttributeValue { S = status });
                conditions["Active"] = titleCondition;
                Dictionary<string, AttributeValue> startKey = null;
                var result = await this.dynamoDBClient.ScanAsync(new ScanRequest()
                {
                    TableName = Constants.TRANSACTION_TABLE_NAME,
                    ExclusiveStartKey = startKey,
                    ScanFilter = conditions
                }).ConfigureAwait(false);

                if (result.Items != null && result.Items.Count > 0)
                {
                    var transactions = new List<Transaction>();
                    foreach (var item in result.Items)
                    {
                        var transaction = new Transaction()
                        {
                            Id = item["Id"].S,
                            BookId = item["BookId"].S,
                            StudentId = item["StudentId"].S,
                            Active = item["Active"].S,
                        };
                        transactions.Add(transaction);
                    }
                    return transactions;
                }
                return null;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<List<Book>> GetBooksByStudentId(string id)
        {
            var books = new List<Book>();

            var transactions = await GetTransactionsByStudentId(id, "true");

            if (transactions == null)
            {
                return books;
            }

            foreach (var transaction in transactions)
            {
                var book = await GetBookById(transaction.BookId);
                books.Add(book);
            }

            return books;
        }

        public async Task<List<Transaction>> GetTransactionsByStudentId(string id, string status)
        {
            try
            {
                Dictionary<string, Condition> conditions = new Dictionary<string, Condition>();
                Condition idCondition = new Condition();
                idCondition.ComparisonOperator = ComparisonOperator.CONTAINS;
                idCondition.AttributeValueList.Add(new AttributeValue { S = id });
                conditions["StudentId"] = idCondition;

                Condition activeCondition = new Condition();
                activeCondition.ComparisonOperator = ComparisonOperator.CONTAINS;
                activeCondition.AttributeValueList.Add(new AttributeValue { S = status });
                conditions["Active"] = activeCondition;

                Dictionary<string, AttributeValue> startKey = null;
                var result = await this.dynamoDBClient.ScanAsync(new ScanRequest()
                {
                    TableName = Constants.TRANSACTION_TABLE_NAME,
                    ExclusiveStartKey = startKey,
                    ScanFilter = conditions
                }).ConfigureAwait(false);

                if (result.Items != null && result.Items.Count > 0)
                {
                    var transactions = new List<Transaction>();
                    foreach (var item in result.Items)
                    {
                        var transaction = new Transaction()
                        {
                            Id = item["Id"].S,
                            StudentId = item["StudentId"].S,
                            BookId = item["BookId"].S,
                            Active = item["Active"].S
                        };
                        transactions.Add(transaction);
                    }
                    return transactions;
                }
                return new List<Transaction>();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<bool> InsertBook(Book book)
        {
            try
            {
                var record = new Document
                {
                    ["Id"] = Guid.NewGuid().ToString(),
                    ["Title"] = book.Title,
                    ["Author"] = book.Author,
                    ["Available"] = "true",
                };

                var dbTable = Table.LoadTable(dynamoDBClient, Constants.BOOK_TABLE_NAME);
                await dbTable.PutItemAsync(record).ConfigureAwait(false);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> InsertTransaction(Transaction transaction)
        {
            try
            {
                var record = new Document
                {
                    ["Id"] = Guid.NewGuid().ToString(),
                    ["StudentId"] = transaction.StudentId,
                    ["BookId"] = transaction.BookId,
                    ["Active"] = "true"
                };
                var dbTable = Table.LoadTable(dynamoDBClient, Constants.TRANSACTION_TABLE_NAME);
                await dbTable.PutItemAsync(record).ConfigureAwait(false);

                await UpdateBookAvailability(transaction.BookId, "false");

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> CloseTransaction(Transaction transaction)
        {
            try
            {
                await UpdateTransactionStatus(transaction.Id, "false");

                await UpdateBookAvailability(transaction.BookId, "true");

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> UpdateTransactionStatus(string id, string status)
        {
            try
            {
                var key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id } },
                };
                var updates = new Dictionary<string, AttributeValueUpdate>();
                updates["Active"] = new AttributeValueUpdate()
                {
                    Action = AttributeAction.PUT,
                    Value = new AttributeValue { S = status }
                };

                var request = new UpdateItemRequest
                {
                    TableName = Constants.TRANSACTION_TABLE_NAME,
                    Key = key,
                    AttributeUpdates = updates
                };

                await dynamoDBClient.UpdateItemAsync(request);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> UpdateBookAvailability(string id, string available)
        {
            try
            {
                var key = new Dictionary<string, AttributeValue>
                {
                    { "Id", new AttributeValue { S = id } },
                };
                var updates = new Dictionary<string, AttributeValueUpdate>();
                updates["Available"] = new AttributeValueUpdate()
                {
                    Action = AttributeAction.PUT,
                    Value = new AttributeValue { S = available }
                };

                var request = new UpdateItemRequest
                {
                    TableName = Constants.BOOK_TABLE_NAME,
                    Key = key,
                    AttributeUpdates = updates
                };

                await dynamoDBClient.UpdateItemAsync(request);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}