How to package and publish a custom DevOps Task to VSTS instance

- obtain a personal token from your vsts account
- build SqlMigrationRunner in release mode
- copy over content from folder 'bin\Release\netcoreapp3.1' to 'DevOps\Task\Console'
- install tfx npm install -g tfx-cli (skip this step you have already installed it)
- open taks.json file and update/increase the version
- open command line and go to the task folder 
- run following command

	tfx build tasks upload --task-path {{REPLACE WITH TASK FOLDER PATH}}\Task --service-url http://{{instance-name}}.visualstudio.com/DefaultCollection

	you should be asked to type in your personal token



How to build a custom task [REFERENCES]

https://docs.microsoft.com/en-us/azure/devops/extend/develop/add-build-task?view=azure-devops
https://github.com/microsoft/azure-pipelines-tasks/blob/master/Tasks/IISWebAppDeploymentOnMachineGroupV0/task.json