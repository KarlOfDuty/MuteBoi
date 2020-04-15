# MuteBoi [![Build Status](http://95.217.45.17:8080/job/MuteBoi/job/master/badge/icon)](http://95.217.45.17:8080/blue/organizations/jenkins/MuteBoi/activity) [![Release](https://img.shields.io/github/release/KarlofDuty/MuteBoi.svg)](https://github.com/KarlOfDuty/MuteBoi/releases) [![Discord Server](https://img.shields.io/discord/430468637183442945.svg?label=discord)](https://discord.gg/C5qMvkj)
Retains specific Discord roles if users leave the server. Useful for muted roles or other permission negating roles. Leaving members are saved in a mysql database with all tracked roles they had when they left.

### Config:

```yaml
bot:
    # Bot token.
    token: "<add-token-here>"
    # Decides what messages are shown in console, possible values are: Critical, Error, Warning, Info, Debug.
    console-log-level: "Info"
    # A list of role ids that should be tracked
    tracked-roles:
      - 111111111111111111
      - 222222222222222222
      - 333333333333333333

database:
    # Address and port of the mysql server
    address: "127.0.0.1"
    port: 3306
    # Name of the database to use
    name: "muteboi"
    # Username and password for authentication
    user: ""
    password: ""
```
