Pass the message with RabbitMQ
==============================
Talking points

Setting up RabbitMQ

	choco install RabbitMQ

	rabbitmq-plugins enable rabbitmq_management

	Install-Package RabbitMQ.Client


Hello World
-----------
Create ConnectionFactory and Connection
Create Channel (= Model in RabbitMQ Client)

Declare Queue

Extension method for converting to/from byte array

BasicPublish a message

Look at management interface, get message


Work queues
-----------
