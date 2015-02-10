function step2_eval(varargin), main(varargin), end

% read
function ret = READ(str)
    ret = reader.read_str(str);
end

% eval
function ret = eval_ast(ast, env)
    switch class(ast)
    case 'types.Symbol'
        ret = env(ast.name);
    case 'types.List'
        ret = types.List();
        for i=1:length(ast)
            ret.append(EVAL(ast.get(i), env));
        end
    case 'types.Vector'
        ret = types.Vector();
        for i=1:length(ast)
            ret.append(EVAL(ast.get(i), env));
        end
    case 'types.HashMap'
        ret = types.HashMap();
        ks = ast.keys();
        for i=1:length(ks)
            k = ks{i};
            ret.set(EVAL(k, env), EVAL(ast.get(k), env));
        end
    otherwise
        ret = ast;
    end
end

function ret = EVAL(ast, env)
    if ~types.list_Q(ast)
        ret = eval_ast(ast, env);
        return;
    end

    % apply
    el = eval_ast(ast, env);
    f = el.get(1);
    args = el.data(2:end);
    ret = f(args{:});
end

% print
function ret = PRINT(ast)
    ret = printer.pr_str(ast, true);
end

% REPL
function ret = rep(str, env)
    ret = PRINT(EVAL(READ(str), env));
end

function main(args)
    repl_env = containers.Map();
    repl_env('+') = @(a,b) a+b;
    repl_env('-') = @(a,b) a-b;
    repl_env('*') = @(a,b) a*b;
    repl_env('/') = @(a,b) floor(a/b);

    %cleanObj = onCleanup(@() disp('*** here1 ***'));
    while (true)
        line = input('user> ', 's');
        if strcmp(strtrim(line),''), continue, end
        try
            fprintf('%s\n', rep(line, repl_env));
        catch err
            fprintf('Error: %s\n', err.message);
            fprintf('%s\n', getReport(err, 'extended'));
        end
    end
end
