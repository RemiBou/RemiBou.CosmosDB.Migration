function StoredProcedure2() {
    var context = getContext();
    var response = context.getResponse();

    response.setBody("Hello, World");
}  