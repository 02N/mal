#!/bin/bash

INTERACTIVE=${INTERACTIVE-yes}

source $(dirname $0)/reader.sh
source $(dirname $0)/printer.sh
source $(dirname $0)/core.sh
source $(dirname $0)/env.sh

# READ: read and parse input
READ () {
    READLINE
    READ_STR "${r}"
}

EVAL_AST () {
    local ast="${1}" env="${2}"
    #_pr_str "${ast}"; echo "EVAL_AST '${ast}:${r} / ${env}'"
    _obj_type "${ast}"; local ot="${r}"
    case "${ot}" in
    symbol)
        local val="${ANON["${ast}"]}"
        ENV_GET "${env}" "${val}"
        return ;;
    list)
        _map_with_type _list EVAL "${ast}" "${env}" ;;
    vector)
        _map_with_type _vector EVAL "${ast}" "${env}" ;;
    hash_map)
        local res="" val="" hm="${ANON["${ast}"]}"
        _hash_map; local new_hm="${r}"
        eval local keys="\${!${hm}[@]}"
        for key in ${keys}; do
            eval val="\${${hm}[\"${key}\"]}"
            EVAL "${val}" "${env}"
            _assoc! "${new_hm}" "${key}" "${r}"
        done
        r="${new_hm}" ;;
    *)
        r="${ast}" ;;
    esac
}

# EVAL: evaluate the parameter
EVAL () {
    local ast="${1}" env="${2}"
    r=
    [[ "${__ERROR}" ]] && return 1
    #_pr_str "${ast}"; echo "EVAL '${r} / ${env}'"
    _obj_type "${ast}"; local ot="${r}"
    if [[ "${ot}" != "list" ]]; then
        EVAL_AST "${ast}" "${env}"
        return
    fi

    # apply list
    _nth "${ast}" 0; local a0="${r}"
    _nth "${ast}" 1; local a1="${r}"
    _nth "${ast}" 2; local a2="${r}"
    case "${ANON["${a0}"]}" in
        def!) local k="${ANON["${a1}"]}"
              #echo "def! ${k} to ${a2} in ${env}"
              EVAL "${a2}" "${env}"
              ENV_SET "${env}" "${k}" "${r}"
              return ;;
        let*) ENV "${env}"; local let_env="${r}"
              local let_pairs=(${ANON["${a1}"]})
              local idx=0
              #echo "let: [${let_pairs[*]}] for ${a2}"
              while [[ "${let_pairs["${idx}"]}" ]]; do
                  EVAL "${let_pairs[$(( idx + 1))]}" "${let_env}"
                  ENV_SET "${let_env}" "${ANON["${let_pairs[${idx}]}"]}" "${r}"
                  idx=$(( idx + 2))
              done
              EVAL "${a2}" "${let_env}"
              return ;;
        *)    EVAL_AST "${ast}" "${env}"
              [[ "${__ERROR}" ]] && r= && return 1
              local el="${r}"
              first "${el}"; local f="${r}"
              rest "${el}"; local args="${ANON["${r}"]}"
              #echo "invoke: ${f} ${args}"
              eval ${f} ${args}
              return ;;
    esac
}

# PRINT:
PRINT () {
    if [[ "${__ERROR}" ]]; then
        _pr_str "${__ERROR}" yes
        r="Error: ${r}"
        __ERROR=
    else
        _pr_str "${1}" yes
    fi
}

# REPL: read, eval, print, loop
ENV; REPL_ENV="${r}"
REP () {
    r=
    READ_STR "${1}"
    EVAL "${r}" ${REPL_ENV}
    PRINT "${r}"
}

_ref () { ENV_SET "${REPL_ENV}" "${1}" "${2}"; }
_ref "+" num_plus
_ref "-" num_minus
_ref "__STAR__" num_multiply
_ref "/" num_divide

if [[ -n "${INTERACTIVE}" ]]; then
    while true; do
        READLINE "user> " || exit "$?"
        [[ "${r}" ]] && REP "${r}" && echo "${r}"
    done
fi
