# BlockChain DNS protocol

The purpose of this protocol is to share ummutable data from a thrusted source (producer) to consumers without having servers.

## Actors

The protocol identifies the following list of actors:

- **Consumer**: who reads data and need to thrust the data he received
- **Producer**: who produces data. He is a thrusted entity and is responsible for security. It is supposed non-malicus.
- **DNS Provider**: a thrusted service that olds a DNS service and is responsible for the security of the DNS system. It is supposed non-malicus.

## Requiremts
The purpose of the protocol is to define a serverless standard for sharing thrusted data across a distribuited system. 

The requirements of the system are:

1. **thrust the source** the data must be get from a thrusted authority. Only the source can write data.
2. **secure** the data must be read only from the consumer.
3. **distributed** the system must be distrubute for beeing esaily accessed by the consumers
4. **serverless** the system dont have to run a server
5. **cost** the system should run theorically free or relies on existing system of a company without wasting resources
6. **any hashing algoritm** we do not impose restriction on hashing algoritm for defining data fingerprints
7. **any crypting algoritms** 
8. **data linking** any record must have an unique, always accessible, URL
9. **data segmentation** data inside an organizaton can be isolated in conainers (database, or collection)

## Protocol

### Phase 1: Hand Shake

This phase can be done in two different mode:

1. Off line handshake
2. Using a thrusted HTTP authority
3. Using a DNS storage

The key point is that the handshake have to be safe and the protocoll assumes that the consumer get the right private key from the producer (no possibility for mad in the middle fraud).

During this process client and producer share the URL of the database, the private key and the criptographic algoritm.



#### 1. Off line handshake
This happens when the consumer and the producer share the key and the communication settings by a secure channel. It can be a cripted archive shared by mail, or being phisically in a room and use voice.


#### 2. Using a thrusted HTTP authority
This happens downloading data using a HTTPS call. The https protocol expose the server certificate, that thrust the source. 
For our porpuse the fact that a server have a certificate issued on a domain is sufficient for considering the source thrusted.
This handshake is valid for data are that are publically available, as the decript key is public.
You can use standard measusre to make this call authenticated.

#### 2. Using a DNS storage
This method is not secure (DNS uses UPD, is public, and not thrusted). Anyway can be used to store a public decript key.
The key is splitted into chunk and published to the url "{index}.db.domain.it" where index is the number of the chunk.




### Phase 2: Data writing
The encrypting\decripting process is based on an asymmetric algorithms. The produces encripts data using a public key, and decripted using a private key by the consumer.
Anyway, because the asymmetric algorithms are limited by the key size and crypting each part would be too expensive (impact of crypting ovehead, huge computational costs) we use the following approach:


The producer can publish data to the BlockChainDNS by the following procedure:

1. The data are in json format. The string representation is without any indentation unicode string.
2. The data contains the following set of special fields:
	- *_history*: an array of the keys of anchestor. The order is from the first to the last (excluded the current one)
3. the data has a textual representation as result of the json serialization. Given the formatting standard and the character encoding, the process is bidirectional and deterministic
4. Key encryption: 
	4.1. the producer generates a simmetric key
	4.2. the key is crypted by the sever with an symmetric algoritm. This protocol doesn not introduce any restriction but the algoritm must be specified during the handshake.
	4.3. the encripted key is converted in base64 and added to the "key" node of the item.
5. Data encription
	5.1. The data is encrypted used the generated key, converted to base64 and added to the "data" node of the json item
6. Unique key generation 
	6.1. the data contains all the information, so an hash of the data is itself a unique key (fingerprint). This can be computed with one of the following alghoritms: MD5, SHA-1, SHA-224, SHA-256, SHA-384 e SHA-512.
	6.2. altering the data will alter the key. The key should be computed with any irreversible algoritm, the protocol do not introduce restriction (but DNS host name lenght may)
7. Data writing
	7.1 bacause a TXT record is limited to 255 character, the content (JSON item with data and key nodes) is splitted into chunks 254 character long. here you can store additional clear information that wont be take in account by the protocol.
	7.2. for each chunk an hash is computed and an index is assigned (integer progessive)
8. Data publishing
	8.1. the hasing data, converted in base32 for being domain complinat, defines the key of the item
	8.2. the key of the element defines the data URL `{key}.{db}.{domain}`
	8.3. each chunk of data is published to a DNS subdomain with URL `{index}.{key}.{db}.{domain}`

### Phase 3: Data read
The read, given an URL can:

1. validate the record integrity, by computing the hasing of data and compating it with the privided key
2. validate the history, fetching all the previous record and validating record integrity
3. thrust the source, because only the producer can write data that are readable with the owned public key



## Solution

The protocol uses the DNS standard as a distribuited database. A client library implements the rules contained into the protocol.

## Prof of concept

This repository contains a fulli working POC.

It uses Shammon database as DNS server (but result can be extended to any public DNS services with API or any on premise solution) and a C# based implmentation that runs on all the major OS (Linux, MacOS, Windows).

### Example


### Performance

Thanks to the simplicity of DNS protocol and the usage of UDP we have this performances:

| benchmark  | time |
| ------------- | ------------- |
| Read  | 6ms  |
| Write*  | 10ms  |

* the write depens on the target DNS system


