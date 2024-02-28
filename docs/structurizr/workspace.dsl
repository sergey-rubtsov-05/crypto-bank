workspace {

    model {
        customer = person "Customer" "A user of my crypto-bank system." {
            tags "External"
        }
        admin = person "Admin" "A user which administrate my crypto-bank system"

        cryptoBank = softwareSystem "Crypto Bank" "My crypto-bank system." {

            customerFrontend = container "User Frontend" "Any frontend app which customer can use to interact with the system." {
                customer -> this "Uses"
            }

            backOfficeFrontend = container "Back Office Frontend" "Any frontend app which admin can use to interact with the system." {
                admin -> this "Uses"
            }

            userService = container "User Service" "User service for my crypto-bank system." {
                userRegistration = component "User Registration" "User registration component."
                userAuth = component "User Auth" "User authentication component."
                userProfile = component "User Profile" "User account component."
                updatePassword = component "Update Password" "Update password component."
                updateRoles = component "Update Roles" "Update roles component." {
                    backOfficeFrontend -> this "Uses"
                }
                customerFrontend -> this "Uses"
            }

            accountService = container "Account Service" "Account service for my crypto-bank system." {
                createAccount = component "Create Account" "Create account component."
                getCustomerAccounts = component "Get Customer Accounts" "Get list of customer accounts."
                transferFundsBetweenAccounts = component "Transfer Funds" "Transfer funds between accounts."
                createVirtualCardAccount = component "Create Virtual Card Account" "Create virtual card with account component."
                getCustomerVirtualCards = component "Get Customer Virtual Cards" "Get list of customer virtual cards."
                transferFundsToCardFromAccount = component "Transfer Funds To Card From Account" "Transfer funds from account to virtual card."
                transferFundsToAccountFromCard = component "Transfer Funds To Account From Card" "Transfer funds from virtual card to account."
                transferFundsBetweenCards = component "Transfer Funds Between Cards" "Transfer funds between virtual cards."
                getTransfersHistory = component "Get Transfers History" "Get transfers history for account."
            }

            qrPaymentService = container "QR Payment Service" "QR payment service for my crypto-bank system." {
                generateQr = component "Generate QR" "Generate QR code for payment."
                scanQr = component "Scan QR" "Scan QR code for payment."
                getQrPaymentsHistory = component "Get QR Payments History" "Get payments history for account."
            }

            btcBlockchainService = container "BTC Blockchain Service" "BTC blockchain service for my crypto-bank system." {
                createDepositAddressBtc = component "Create Deposit Address" "Create deposit address for account."
                getDepositBtc = component "Get Deposit" "Get deposit for account."
                approveDepositBtc = component "Approve Deposit" "Approve deposit for account."
                createWithdrawalBtc = component "Create Withdrawal" "Create withdrawal for account."
                approveWithdrawalBtc = component "Approve Withdrawal" "Approve withdrawal for account."
                notifyToSignalRBtc = component "Notify To SignalR" "Notify to SignalR for account."
                notifyToEmailBtc = component "Notify To Email" "Notify to email for account."
            }

            ethBlockchainService = container "ETH Blockchain Service" "ETH blockchain service for my crypto-bank system." {
                createDepositAddressEth = component "Create Deposit Address" "Create deposit address for account."
                getDepositEth = component "Get Deposit" "Get deposit for account."
                approveDepositEth = component "Approve Deposit" "Approve deposit for account."
                createWithdrawalEth = component "Create Withdrawal" "Create withdrawal for account."
                approveWithdrawalEth = component "Approve Withdrawal" "Approve withdrawal for account."
                notifyToSignalREth = component "Notify To SignalR" "Notify to SignalR for account."
                notifyToEmailEth = component "Notify To Email" "Notify to email for account."
            }

            utilityProviderService = container "Utility Provider Service" "Utility provider service for my crypto-bank system." {
                loadUnpaidBillsJob = component "Load Unpaid Bills Job" "Periodically load unpaid bills for account."
                getUppaidBills = component "Get Unpaid Bills" "Get unpaid bills for account."
                payBill = component "Pay Bill" "Pay bill for account."
                getPaymentsHistory = component "Get Payments History" "Get payments history for account."
                notifyToSignalR = component "Notify To SignalR" "Notify to SignalR for account."
                notifyToEmail = component "Notify To Email" "Notify to email for account."
            }

            reportService = container "Report Service" "Report service for my crypto-bank system." {
                generateOpennedAccountsForPeriodReport = component "Generate Openned Accounts For Period Report" "Generate openned accounts for period report." {
                    backOfficeFrontend -> this
                }
                generateCryptoCurrencyInOutReport = component "Generate Crypto Currency In Out Report" "Generate crypto currency in out report." {
                    backOfficeFrontend -> this
                }
            }
        }

        softwareSystem "BTC blockchain" {
            tags "External"
            btcBlockchainService -> this "Uses"
        }

        softwareSystem "ETH blockchain" {
            tags "External"
            ethBlockchainService -> this "Uses"
        }

        utilityProvider = softwareSystem "UtilityProvider" "Providers features for get and pay utility bills" {
            tags "External"
            utilityProviderService -> this "Uses"
        }
    }

    views {
        systemContext cryptoBank "SystemContext" {
            include *
            autoLayout
        }

        container cryptoBank {
            include *
            autoLayout
        }

        component userService {
            include *
            autoLayout
        }

        component accountService {
            include *
            autoLayout
        }

        component qrPaymentService {
            include *
            autoLayout
        }

        component btcBlockchainService {
            include *
            autoLayout
        }

        component ethBlockchainService {
            include *
            autoLayout
        }

        component utilityProviderService {
            include *
            autoLayout
        }

        component reportService {
            include *
            autoLayout
        }

        theme default

        styles {
            element "External" {
                background #dddddd
                color #000000
            }
        }
    }

}