--executed = 0
--received = 0
--
--local function Loop()
--    SetTimeout(2, function ()
----        Postgres.Sync.fetchAll('WRONG SQL QUERY', {})
--
--        Postgres.Sync.fetchScalar('SELECT @parameters', {
--            ['@parameters'] =  'string'
--        })
--
--        executed = executed + 1
--
--        Postgres.Async.fetchAll('SELECT "hello2" as world', {}, function(result)
--            received = received + 1
--        end)
--
--        if executed % 100 == 0 then
--            print(received .. "/"  .. executed)
--        end
--
--        Loop()
--    end)
--end
--
--AddEventHandler('onPostgresReady', function ()
--    Loop()
--end)

Postgres.ready(function ()
    print(Postgres.Sync.fetchScalar('SELECT @parameters', {
        ['@parameters'] =  1
    }))

    print(Postgres.Sync.fetchScalar('SELECT @parameters', {
        ['@parameters'] =  'string'
    }))

    Postgres.Async.fetchScalar('SELECT NOW() as world', {}, function(result)
        print(result)
    end)

    Postgres.Async.execute('SELECT pg_sleep(5)', nil, function()
        print("1")
    end)
    Postgres.Async.execute('SELECT pg_sleep(4)', nil, function()
        print("2")
    end)
    Postgres.Async.execute('SELECT pg_sleep(3)', {}, function()
        print("3")
    end)
    Postgres.Async.execute('SELECT pg_sleep(2)', nil, function()
        print("4")
    end)
    Postgres.Async.execute('SELECT pg_sleep(1)', nil, function()
        print("5")
    end)

    print(Postgres.Sync.fetchAll("SELECT 'hello1' as world", {})[1].world)

    Postgres.Async.fetchAll("SELECT 'hello2' as world", {}, function(result)
        print(result[1].world)
    end)

    print(Postgres.Sync.fetchScalar("SELECT 'hello3' as world", {}))

    Postgres.Async.fetchScalar("SELECT 'hello4' as world", {}, function(result)
        print(result)
    end)

    print(json.encode(Postgres.Sync.fetchScalar('SELECT null', {})))

    Postgres.Async.fetchScalar('SELECT null', {}, function(result)
        print(result)
    end)

    Postgres.Sync.fetchAll('WRONG SQL QUERY', {})
end)
