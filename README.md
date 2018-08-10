# Postgres Async Library for FiveM

This library intends to provide function to connect to a Postgres library in a Sync and Async Way.

## Disclaimer

This mod does not replace EssentialMode, it offers instead a new way of connecting to Postgres, but
it will never contain any gameplay logic. It will remain a simple wrapper around Postgres functions.

All feedback is appreciated in order to deliver a stable release.

## Installation

Install the content of this repository in the `resources/postgres-async` folder. **Name of the folder** matters, 
do not use a different name (otherwise you must have knowledge on how this works and make the appropriate changes)

Once installed, you will need to add this line of code in the resource file of each mod needing a Postgres client:

```
server_script '@postgres-async/lib/Postgres.lua'
```

## Configuration

Add this convar to your server configuration and change the values according to your Postgres installation:

`set psql_connection_string "Host=127.0.0.1;Username=myusername;Password=1202;Database=fivem"`

## Usage

### Waiting for Postgres to be ready

You need to encapsulate your code into `Postgres.ready` to be sure that the mod will be available and initialized
before your first request.

```lua
Postgres.ready(function ()
    print(Postgres.Sync.fetchScalar('SELECT @parameters', {
        ['@parameters'] =  'string'
    }))
end)
```

### Sync

> Sync functions can block the main thread, always prefer the Async version if possible, there is very rare 
> use case for you to use this.

#### Postgres.Sync.execute(string query, array params) : int

Execute a postgres query which should not send any result (like a Insert / Delete / Update), and will return the 
number of affected rows.

```lua
Postgres.Sync.execute("UPDATE player SET name=@name WHERE id=@id", {['@id'] = 10, ['@name'] = 'foo'})
```

#### Postgres.Sync.fetchAll(string query, array params) : object[]

Fetch results from Postgres and returns them in the form of an Array of Objects:

```lua
local players = Postgres.Sync.fetchAll('SELECT id, name FROM player')
print(players[1].id)
```

#### Postgres.Sync.fetchScalar(string query, array params) : mixed

Fetch the first field of the first row in a query:

```lua
local countPlayer = Postgres.Sync.fetchScalar("SELECT COUNT(1) FROM players")
```

### Async

#### Postgres.Async.execute(string query, array params, function callback)

Works like `Postgres.Sync.execute` but will return immediatly instead of waiting for the execution of the query.
To exploit the result of an async method you must use a callback function:

```lua
Postgres.Async.execute('SELECT SLEEP(10)', {}, function(rowsChanged)
    print(rowsChanged)
end)
```

#### Postgres.Async.fetchAll(string query, array params, function callback)

Works like `Postgres.Sync.fetchAll` and provide callback like the `Postgres.Async.execute` method:

```lua
Postgres.Async.fetchAll('SELECT * FROM player', {}, function(players)
    print(players[1].name)
end)
```

#### Postgres.Async.fetchScalar(string query, array params, function callback)

Same as before for the fetchScalar method.

```lua
Postgres.Async.fetchScalar("SELECT COUNT(1) FROM players", function(countPlayer)
    print(countPlayer)
end
```

## Features

 * Async / Sync
 * It uses the [Npgsql](https://github.com/npgsql/npgsql) library
 * Create and close a connection for each query, the underlying library use a connection pool so only the 
postgres auth is done each time, old tcp connections are keeped in memory for performance reasons (I think?)

## Credits

The WHOLE library even this README, is a rewritten(search/replace) version of [FiveM-MySQL-Async](https://github.com/brouznouf/fivem-mysql-async), big thanks to [Joel Wurtz](https://github.com/joelwurtz) for creating FiveM-MySQL-Async, without it this wouldn't be possible, for me atleast ;)
