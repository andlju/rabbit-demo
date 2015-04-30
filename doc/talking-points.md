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

BasicPublish

Run the app
Look at management interface, get message. Don't requeue.

BasicGet - don't Ack.
Run the app

Close app. Look at management interface. Message is back?
Purge queue.

BasicAck
Run the app. Get message. Ack message. Queue should


Work queues
-----------
Show producer code.

Show consumer code. 


*** Multiple Producers
Start one producer. Give name "First producer". Send 5 messages.

Start one consumer. Watch as they are consumed.

Start new producer. Give name Second producer.
 
Send 10 messages from First.

Send 5 messages from Second.

Watch them being consumed in order.

*** Multiple Consumers
