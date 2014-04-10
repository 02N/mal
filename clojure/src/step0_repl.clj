(ns step0-repl
  (:require [readline]))


;; read
(defn READ [& [strng]]
  (let [line (if strng strng (read-line))]
    strng))

;; eval
(defn EVAL [ast env]
  (eval (read-string ast)))

;; print
(defn PRINT [exp] exp)

;; repl
(defn rep [strng] (PRINT (EVAL (READ strng), {})))

(defn -main [& args]
  (loop []
    (let [line (readline/readline "user> ")]
      (when line
        (println (rep line))
        (recur)))))
