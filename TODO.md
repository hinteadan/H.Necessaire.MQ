# Stuff TODO

Add resiliency (re-establish listening connection)
 - For RabbitMQ
	- For QD
	- For HMQ
 - For Azure SB
	- For QD
	- For HMQ

Cache actionQer connection for when sending multiple messages in a short amount of time, to no close and open the connection for each send
 - For RabbitMQ
	- For QD
	- For HMQ
 - For Azure SB
	- For QD
	- For HMQ

Reuse connection for sending message if the process has an open connection for listening
 - Use 1 connection per node, multiple channels (for HMQ and QD)

Sagas