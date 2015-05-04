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
One or more producers that create tasks.
One or more consumers that process tasks.

Show producer code.

Show consumer code. 


*** Multiple Producers
Start one producer. Give name "First producer". Send 5 messages.

Start one consumer. Watch as they are processed.

Start new producer. Give name Second producer.
 
Send 10 messages from First.

Send 5 messages from Second.

Watch them being processed in order.

*** Multiple Consumers

Close Second producer

Send 100 messages, watch Consumer start processing

Start new consumer, nothing happens?

Set Prefetch to 1

Start two consumers


Publish/Subscribe
-----------------
One or more publishers that publishes messages
One or more subscribers that want to be notified on all events


Rabbit always publishes to an exchange.

 

Error handling
--------------
Rabbit connection can (and will) go down. Throws exception. Should reconnect.



Clustering
----------

*** Load balancing
