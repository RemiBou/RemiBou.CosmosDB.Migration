function StoredProcedure3() {
    var context = getContext();
    var response = context.getResponse();

    response.setBody("Hello, World");
}  