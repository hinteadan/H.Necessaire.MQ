# Stuff TODO

Add resiliency (re-establish listening connection)
 - For RabbitMQ
 - For Azure SB

Cache actionQer connection for when sending multiple messages in a short amount of time, to no close and open the connection for each send
 - For RabbitMQ
 - For Azure SB

Reuse conenction for sending message if the process has an open connection for listening

Sagas