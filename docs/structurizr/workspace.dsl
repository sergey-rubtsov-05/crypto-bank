workspace {

    model {
        user = person "Client" "A user of my crypto-bank system." {
            tags "External"
        }
        admin = person "Admin" "A user which administrate my crypto-bank system"

        cryptoBank = softwareSystem "Crypto Bank" "My crypto-bank system." {
            userService = container "User Service" "User service for my crypto-bank system." {
                userRegistration = component "User Registration" "User registration component." {
                    user -> this "Creates profile"
                }
                userLogin = component "User Login" "User login component." {
                    user -> this "Login to system"
                }
                userAccount = component "User Account" "User account component."
                updateRoles = component "Update Roles" "Update roles component." {
                    admin -> this "Uses"
                }
            }
        }

        softwareSystem "BTC blockchain" {
            tags "External"
            cryptoBank -> this "Uses"
        }

        softwareSystem "ETH blockchain" {
            tags "External"
            cryptoBank -> this "Uses"
        }

        softwareSystem "UtilityProvider" "Providers features for get and pay utility bills" {
            tags "External"
            cryptoBank -> this "Uses"
        }
    }

    views {
        systemContext cryptoBank "SystemContext" {
            include *
            autoLayout
        }

        container cryptoBank {
            include * user
            autoLayout
        }
        
        component userService {
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