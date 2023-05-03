# HartreeTaskSuite

The HartreeTaskSuite consists of the following projects addressing the test. Details of the projects and documents in the folder are listed below. 

\HartreeTaskSuite\Benchmarking 
    A console application that I was using to do some benchmarking

\HartreeTaskSuite\Consumer
    Console application that subscribes to Kafka topic for messages and persists them into MSSql
    Consumer is dependent on MessageSink, MessageConsumer and TickData
    
\HartreeTaskSuite\MessageConsumer
    Library that handles Subscription code to Kafka
    
\HartreeTaskSuite\MessagePublisher
    Library that handles Publishing code to Kafka
    
\HartreeTaskSuite\MessageSink
    Library to persist data to MS Sql. 
    It supports both vanilla bulk insert queries and EntityFramework solution to persist data.
    What solution to use is to be configured in Consumer's app.config
    
\HartreeTaskSuite\Producer
    Console application to public 2 ticks per second to Kafka.
    Producer uses MessagePublisher and TickData.
    TickFrequency and TickCount are configurable from app.config of Producer
    
\HartreeTaskSuite\TestSuiteApps
    Test project for testing development
    
\HartreeTaskSuite\TickData
    Data object with Json serializers and deserializers.
    
\HartreeTaskSuite\.editorconfig and \HartreeTaskSuite\HartreeTaskSuite.sln
    VisualStudio and Solution files respectively
    
\RefLinks.txt
    Googled links that I referenced during the development

\RequirementAnalysis.txt

\Run Producer and Consumer.docx
    A word document detailing how to configure and run the binaries.
    
\csharp_test.txt
    The test received in email.
