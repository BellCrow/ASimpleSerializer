# ASimpleSerializer
A very basic serializer for serializing arbitrary sets of datatypes in csharp.

Was written for inter computer communication in the game space engineers.
Which is also the reason, why its all crammed up in one sourcefile.

# Properties
- One class for serializing
  - Will collect subsequent data from calls to serialization for primitive datatypes such as
    ** long,int,string,bool and double **
  - after a call to FinalizeSerialization() a string will be returned containing all data
- One class for deserializing
